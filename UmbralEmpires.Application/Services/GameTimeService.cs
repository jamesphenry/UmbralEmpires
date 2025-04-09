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
        if (playerBase.ConstructionQueue == null || !playerBase.ConstructionQueue.Any()) return;

        bool baseStateChanged = false;
        bool playerStateChanged = false;
        double remainingSecondsInTick = secondsToAdvance;

        while (remainingSecondsInTick > 0.001 && playerBase.ConstructionQueue.Any())
        {
            var activeItem = playerBase.ConstructionQueue[0];
            bool itemHasStartedBuilding = activeItem.RemainingBuildTimeSeconds < activeItem.TotalBuildTimeSeconds; // Infer if countdown has begun

            // --- Check if Paused (Needs Credits to Start/Resume) ---
            // We infer 'paused' if item is at front but hasn't started (Remaining == Total)
            // Or if an explicit Pause flag is set (TODO: Add Pause flag to Base entity?)
            bool requiresCreditCheck = !itemHasStartedBuilding || IsQueuePaused(playerBase); // Check if not started OR explicitly paused

            if (requiresCreditCheck)
            {
                int requiredCredits = StructureDataLookup.GetCreditCostForLevel(activeItem.StructureType, activeItem.TargetLevel);
                if (player.Credits >= requiredCredits)
                {
                    // Can afford to start/resume
                    if (!itemHasStartedBuilding) // Only deduct credits once when starting
                    {
                        if (player.SpendCredits(requiredCredits)) // Ensure SpendCredits returns bool success
                        {
                            activeItem.RemainingBuildTimeSeconds = activeItem.TotalBuildTimeSeconds; // Initialize timer
                            _logger.LogInformation("Starting construction: {Structure} Lvl {Target} at Base {BaseId}",
                                activeItem.StructureType, activeItem.TargetLevel, playerBase.Id);
                            playerStateChanged = true;
                            baseStateChanged = true; // Queue item state changed
                        }
                        else
                        {
                            // Should not happen if Credits >= requiredCredits, but safety check
                            _logger.LogError("Failed to spend credits despite check passing for Player {PlayerId}", player.Id);
                            break; // Stop processing if credit spending fails unexpectedly
                        }
                    }
                    SetQueuePaused(playerBase, false); // Ensure queue is not marked as paused
                }
                else
                {
                    // Cannot afford, ensure queue is paused and stop processing for this tick
                    SetQueuePaused(playerBase, true);
                    _logger.LogDebug("Construction queue paused at Base {BaseId} (Credits: {Current}/{Required})",
                        playerBase.Id, player.Credits, requiredCredits);
                    break;
                }
            }

            // If queue is running and item has started...
            if (!IsQueuePaused(playerBase) && activeItem.RemainingBuildTimeSeconds > 0)
            {
                double timeToComplete = activeItem.RemainingBuildTimeSeconds;
                double timeToApply = Math.Min(remainingSecondsInTick, timeToComplete);

                activeItem.RemainingBuildTimeSeconds -= timeToApply;
                remainingSecondsInTick -= timeToApply;
                baseStateChanged = true; // Queue item state changed

                // --- Check for Completion ---
                if (activeItem.RemainingBuildTimeSeconds <= 0.001)
                {
                    _logger.LogInformation("Construction completed: {Structure} Lvl {Target} at Base {BaseId}",
                        activeItem.StructureType, activeItem.TargetLevel, playerBase.Id);

                    playerBase.CompleteFirstQueueItem(); // Removes item, updates Base.Structures

                    // Immediately loop to potentially start the next item if time remains in tick
                }
            }
            else if (!IsQueuePaused(playerBase) && activeItem.RemainingBuildTimeSeconds <= 0.001 && playerBase.ConstructionQueue.Any())
            {
                // This means item completed exactly on previous tick or somehow has 0 time but is still here.
                // This might happen if CompleteFirstQueueItem wasn't called correctly, or state is odd.
                // Force completion/removal just in case to avoid infinite loops.
                _logger.LogWarning("Queue item {Structure} Lvl {Target} had <= 0 time remaining but was still at front. Forcing completion.", activeItem.StructureType, activeItem.TargetLevel);
                playerBase.CompleteFirstQueueItem();
                baseStateChanged = true;
            }
            else if (!IsQueuePaused(playerBase) && !playerBase.ConstructionQueue.Any())
            {
                // Queue became empty during processing (last item completed)
                break;
            }
            // Implicitly handles the case where queue is paused and we break out.
        }

        // Persist changes outside the loop if any occurred
        if (baseStateChanged)
        {
            await _baseRepository.UpdateAsync(playerBase);
        }
        if (playerStateChanged)
        {
            await _playerRepository.UpdateAsync(player);
        }
    }

    // Placeholder methods for queue pause state - needs implementation detail (e.g., bool flag on Base?)
    private bool IsQueuePaused(Base playerBase) => false; // TODO: Implement check
    private void SetQueuePaused(Base playerBase, bool isPaused) { /* TODO: Implement state change */ }
}


