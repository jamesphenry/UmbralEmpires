using UmbralEmpires.Application.Persistence;
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using Microsoft.Extensions.Logging; // Optional logging
using System;
using System.Linq;
using System.Threading.Tasks;
using UmbralEmpires.Application.GameData;

namespace UmbralEmpires.Application.Services;

public class GameTimeService : IGameTimeService
{
    // --- Dependencies (Injected via Constructor) ---
    private readonly IBaseRepository _baseRepository;
    private readonly IAstroRepository _astroRepository;
    private readonly IPlayerRepository _playerRepository; // Assuming we create this soon
    private readonly IBaseStatsCalculatorService _statsCalculator;
    private readonly ILogger<GameTimeService> _logger;
    // private readonly IUnitOfWork _unitOfWork; // For SaveChangesAsync coordination

    public GameTimeService(
        IBaseRepository baseRepository,
        IAstroRepository astroRepository,
        IPlayerRepository playerRepository,
        IBaseStatsCalculatorService statsCalculator,
        ILogger<GameTimeService> logger
        /* IUnitOfWork unitOfWork */)
    {
        _baseRepository = baseRepository;
        _astroRepository = astroRepository;
        _playerRepository = playerRepository;
        _statsCalculator = statsCalculator;
        _logger = logger;
        // _unitOfWork = unitOfWork;
    }

    public async Task TickAsync(Guid playerId, double elapsedRealTimeSeconds, double gameHoursPerRealSecond = 1.0)
    {
        double elapsedGameHours = elapsedRealTimeSeconds * gameHoursPerRealSecond;
        if (elapsedGameHours <= 0) return; // Nothing to process

        _logger.LogDebug("Processing game tick for Player {PlayerId}. Real seconds: {RealSec}, Game hours: {GameHrs}",
            playerId, elapsedRealTimeSeconds, elapsedGameHours);

        // --- 1. Load necessary state ---
        var player = await _playerRepository.GetByIdAsync(playerId); // Need Player entity
        if (player == null) { /* Log error, return */ return; }
        var bases = (await _baseRepository.GetByPlayerIdAsync(playerId)).ToList();

        double totalIncome = 0;
        double constructionTimeAdvanceSeconds = elapsedGameHours * 3600; // Game hours -> Game seconds

        // --- 2. Process each base ---
        foreach (var playerBase in bases)
        {
            var astro = await _astroRepository.GetByIdAsync(playerBase.AstroId);
            if (astro == null) { /* Log warning, continue */ continue; }

            var stats = _statsCalculator.CalculateStats(playerBase, astro); // Calc stats at start of tick

            // --- 2a. Calculate Income ---
            totalIncome += stats.BaseEconomy * elapsedGameHours;

            // --- 2b. Process Construction Queue ---
            await ProcessConstructionQueueAsync(player, playerBase, stats, constructionTimeAdvanceSeconds);
        }

        // --- 3. Update Player State ---
        player.AddCredits((int)Math.Floor(totalIncome)); // Need Player entity method
        await _playerRepository.UpdateAsync(player);

        // --- 4. Save All Changes ---
        // This should happen here or be coordinated by the caller using Unit of Work
        // await _unitOfWork.SaveChangesAsync(); // Example

        _logger.LogDebug("Tick complete for Player {PlayerId}. Added Income: {Income}", playerId, totalIncome);
    }

    private async Task ProcessConstructionQueueAsync(Player player, Base playerBase, CalculatedBaseStats currentStats, double secondsToAdvance)
    {
        if (playerBase.ConstructionQueue == null || playerBase.ConstructionQueue.Count == 0) return;

        // Need to track remaining time persistently. Let's add it to ConstructionQueueItem record.
        // Modify ConstructionQueueItem: record(..., double TotalBuildTimeSeconds, double RemainingBuildTimeSeconds);

        bool baseStateChanged = false;
        double remainingSecondsInTick = secondsToAdvance;

        while (remainingSecondsInTick > 0.001 && playerBase.ConstructionQueue.Count > 0) // Use small epsilon
        {
            var activeItem = playerBase.ConstructionQueue[0];

            // Check if queue is paused (e.g., needs credits)
            if (IsQueuePaused(playerBase)) // Need mechanism to check/set pause state
            {
                // Attempt to restart if credits now sufficient
                int requiredCredits = StructureDataLookup.GetCreditCostForLevel(activeItem.StructureType, activeItem.TargetLevel);
                if (player.Credits >= requiredCredits)
                {
                    player.SpendCredits(requiredCredits); // Need method on Player
                    SetQueuePaused(playerBase, false); // Unpause
                                                       // Mark activeItem as started if not already? How to track? Maybe Remaining = Total if just starting.
                    if (activeItem.RemainingBuildTimeSeconds <= 0) // Check if it hasn't started counting down
                        activeItem.RemainingBuildTimeSeconds = activeItem.TotalBuildTimeSeconds;

                    _logger.LogInformation("Construction queue resumed at Base {BaseId}.", playerBase.Id);
                    baseStateChanged = true;
                }
                else
                {
                    _logger.LogDebug("Construction queue remains paused at Base {BaseId} (Credits: {Current}/{Required})",
                       playerBase.Id, player.Credits, requiredCredits);
                    break; // Can't proceed, stop processing queue for this tick
                }
            }

            // If it was just unpaused or already running
            if (activeItem.RemainingBuildTimeSeconds <= 0) activeItem.RemainingBuildTimeSeconds = activeItem.TotalBuildTimeSeconds; // Ensure timer starts if needed


            // Process active item
            double timeToComplete = activeItem.RemainingBuildTimeSeconds;
            double timeToApply = Math.Min(remainingSecondsInTick, timeToComplete);

            activeItem.RemainingBuildTimeSeconds -= timeToApply;
            remainingSecondsInTick -= timeToApply;
            baseStateChanged = true;

            // Check for completion
            if (activeItem.RemainingBuildTimeSeconds <= 0.001) // Use epsilon
            {
                _logger.LogInformation("Construction completed: {Structure} Level {Level} at Base {BaseId}",
                    activeItem.StructureType, activeItem.TargetLevel, playerBase.Id);

                playerBase.CompleteFirstQueueItem(); // Updates Structures dict, removes from queue

                // Invalidation check/handling might be needed here or before starting next

                // No need to explicitly start next item here; loop will handle it if time remains
            }
            // If item did not complete, loop continues with reduced remainingSecondsInTick
            // If item did complete, loop continues and will check next item (if any)
        }

        if (baseStateChanged)
        {
            await _baseRepository.UpdateAsync(playerBase); // Persist changes to Base (Queue, Structures)
        }
    }

    // Placeholder methods for queue pause state - needs implementation detail (e.g., bool flag on Base?)
    private bool IsQueuePaused(Base playerBase) => false; // TODO: Implement check
    private void SetQueuePaused(Base playerBase, bool isPaused) { /* TODO: Implement state change */ }
}


// --- Need Player entity and IPlayerRepository ---
public class Player
{
    public Guid Id { get; set; }
    public int Credits { get; private set; }
    public void AddCredits(int amount) { if (amount > 0) Credits += amount; }
    public bool SpendCredits(int amount)
    {
        if (amount > 0 && Credits >= amount) { Credits -= amount; return true; }
        return false;
    }
}
public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task UpdateAsync(Player player);
}
// --- Need Updated ConstructionQueueItem ---
// public record ConstructionQueueItem(
//     StructureType StructureType,
//     int TargetLevel,
//     double TotalBuildTimeSeconds,
//     double RemainingBuildTimeSeconds // Added for persistent tracking
// );