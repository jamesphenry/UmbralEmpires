using UmbralEmpires.Application.Persistence; // Repositories
using UmbralEmpires.Application.GameData;    // StructureDataLookup
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace UmbralEmpires.Application.Services;

public class ConstructionService : IConstructionService
{
    private readonly IBaseRepository _baseRepository;
    private readonly IAstroRepository _astroRepository;
    private readonly IBaseStatsCalculatorService _statsCalculator;
    private readonly ILogger<ConstructionService> _logger;
    // Inject IUnitOfWork later for SaveChangesAsync

    public ConstructionService(
        IBaseRepository baseRepository,
        IAstroRepository astroRepository,
        IBaseStatsCalculatorService statsCalculator,
        ILogger<ConstructionService> logger)
    {
        _baseRepository = baseRepository;
        _astroRepository = astroRepository;
        _statsCalculator = statsCalculator;
        _logger = logger;
    }

    public async Task<QueueResult> QueueStructureAsync(Guid playerId, Guid baseId, StructureType structureToBuild)
    {
        // --- 1. Load Entities ---
        var playerBase = await _baseRepository.GetByIdAsync(baseId);
        if (playerBase == null || playerBase.PlayerId != playerId)
            return new QueueResult(false, "Base not found or access denied.");

        var astro = await _astroRepository.GetByIdAsync(playerBase.AstroId);
        if (astro == null)
            return new QueueResult(false, "Astro data not found for base.");

        // --- 2. Check Queue Limit ---
        const int queueLimit = 5;
        if (playerBase.ConstructionQueue.Count >= queueLimit)
            return new QueueResult(false, $"Construction queue full (Max {queueLimit}).");

        // --- 3. Determine Target Level & Check Max ---
        int currentLevel = playerBase.Structures.TryGetValue(structureToBuild, out var lvl) ? lvl : 0;
        int targetLevel = currentLevel + 1;
        // TODO: Implement check against max level for 'structureToBuild' from StructureDataLookup?

        // --- 4. Prerequisite Checks ---
        // TODO: Implement Tech prerequisite check (using StructureDataLookup & PlayerTech state) - Skip for M1

        // --- 5. Resource Limit Validation (Using CURRENT stats - Simplified M1 approach) ---
        // Calculate current stats BEFORE considering the new structure level
        var currentStats = _statsCalculator.CalculateStats(playerBase, astro);

        // Check Energy
        int energyReqPerLevel = StructureDataLookup.GetEnergyRequirementPerLevel(structureToBuild);
        // Check if adding one more level's requirement exceeds current production buffer
        if (currentStats.EnergyConsumption + energyReqPerLevel > currentStats.EnergyProduction)
            return new QueueResult(false, $"Insufficient Energy (Needs {energyReqPerLevel}, Available: {currentStats.EnergyProduction - currentStats.EnergyConsumption})");

        // Check Population
        int popReqPerLevel = StructureDataLookup.GetPopulationRequirementPerLevel(structureToBuild);
        if (currentStats.CurrentPopulationUsed + popReqPerLevel > currentStats.MaxPopulation)
            return new QueueResult(false, $"Insufficient Population (Needs {popReqPerLevel}, Available: {currentStats.MaxPopulation - currentStats.CurrentPopulationUsed})");

        // Check Area
        int areaReqPerLevel = StructureDataLookup.GetAreaRequirementPerLevel(structureToBuild);
        if (currentStats.CurrentAreaUsed + areaReqPerLevel > currentStats.MaxArea)
            return new QueueResult(false, $"Insufficient Area (Needs {areaReqPerLevel}, Available: {currentStats.MaxArea - currentStats.CurrentAreaUsed})");

        // NOTE: Predictive validation (estimating future state) is more complex and deferred.
        // This simple check assumes resources available *now* are sufficient.

        // --- 6. Calculate Build Time & Cost ---
        int creditCost = StructureDataLookup.GetCreditCostForLevel(structureToBuild, targetLevel);
        if (currentStats.ConstructionCapacity <= 0)
            return new QueueResult(false, "Construction capacity is zero or negative.");
        double buildTimeSeconds = (double)creditCost / currentStats.ConstructionCapacity * 3600;
        if (buildTimeSeconds <= 0 || double.IsNaN(buildTimeSeconds) || double.IsInfinity(buildTimeSeconds))
            return new QueueResult(false, "Invalid build time calculated (Cost or Capacity issue).");

        // --- 7. Create Queue Item ---
        var queueItem = new ConstructionQueueItem(
            structureToBuild,
            targetLevel,
            buildTimeSeconds
        ); // RemainingTime initialized in constructor

        // --- 8. Add to Queue & Save ---
        try
        {
            playerBase.AddToConstructionQueue(queueItem); // Use method on Base entity
            await _baseRepository.UpdateAsync(playerBase); // Mark Base as modified
            // await _unitOfWork.SaveChangesAsync(); // Commit transaction (later)

            _logger.LogInformation("Queued {Structure} Lvl {Level} at Base {BaseId}. Cost: {Cost}, Time: {Time:F0}s",
                structureToBuild, targetLevel, baseId, creditCost, buildTimeSeconds);
            return new QueueResult(true, $"{structureToBuild} Level {targetLevel} added to queue.");
        }
        catch (InvalidOperationException qex) // Catch queue full exception potentially thrown by AddToConstructionQueue
        {
            _logger.LogWarning(qex, "Failed to add to queue for Base {BaseId} (Queue may be full).", baseId);
            return new QueueResult(false, qex.Message);
        }
        catch (Exception ex) // Catch DB update or other errors
        {
            _logger.LogError(ex, "Error queueing construction for Base {BaseId}", baseId);
            return new QueueResult(false, "An unexpected error occurred.");
        }
    }
}