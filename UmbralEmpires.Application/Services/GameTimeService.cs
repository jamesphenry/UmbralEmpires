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
        // If queue is empty, ensure not paused and return
        if (playerBase.ConstructionQueue == null || !playerBase.ConstructionQueue.Any())
        {
            if (playerBase.IsConstructionPaused)
            {
                playerBase.SetConstructionPaused(false);
                await _baseRepository.UpdateAsync(playerBase); // Persist unpause state if queue empty
            }
            return;
        }

        bool baseStateChanged = false;
        bool playerStateChanged = false;
        double remainingSecondsInTick = secondsToAdvance;

        // Only process if time available and queue has items
        while (remainingSecondsInTick > 0.001 && playerBase.ConstructionQueue.Any())
        {
            var activeItem = playerBase.ConstructionQueue[0];
            // Infer if item needs starting procedure (credits check/deduction)
            bool needsToStart = activeItem.RemainingBuildTimeSeconds >= activeItem.TotalBuildTimeSeconds;

            // --- Check Pause State OR Attempt to Start Item ---
            if (playerBase.IsConstructionPaused || needsToStart)
            {
                int requiredCredits = StructureDataLookup.GetCreditCostForLevel(activeItem.StructureType, activeItem.TargetLevel);
                if (player.Credits >= requiredCredits)
                {
                    // Can afford to start/resume
                    if (needsToStart) // Only deduct credits if it hasn't started yet
                    {
                        if (player.SpendCredits(requiredCredits))
                        {
                            activeItem.RemainingBuildTimeSeconds = activeItem.TotalBuildTimeSeconds; // Initialize timer
                            _logger.LogInformation("Starting construction: {Structure} Lvl {Target} at Base {BaseId}", activeItem.StructureType, activeItem.TargetLevel, playerBase.Id);
                            playerStateChanged = true;
                            baseStateChanged = true;
                        }
                        else
                        {
                            _logger.LogError("Failed to spend credits for {Structure} Lvl {Target} despite check passing for Player {PlayerId}", activeItem.StructureType, activeItem.TargetLevel, player.Id);
                            // Should we pause here or just log error and proceed? Let's pause.
                            playerBase.SetConstructionPaused(true);
                            baseStateChanged = true;
                            break; // Stop processing if spend fails unexpectedly
                        }
                    }
                    // If we successfully started or resumed, ensure pause flag is off
                    if (playerBase.IsConstructionPaused)
                    {
                        playerBase.SetConstructionPaused(false);
                        _logger.LogInformation("Construction queue resumed at Base {BaseId}.", playerBase.Id);
                        baseStateChanged = true;
                    }
                }
                else // Cannot afford
                {
                    // Ensure queue is paused and stop processing for this tick
                    if (!playerBase.IsConstructionPaused)
                    { // Only log/set if not already paused
                        playerBase.SetConstructionPaused(true);
                        _logger.LogInformation("Construction queue PAUSED at Base {BaseId} (Credits: {Current}/{Required})", playerBase.Id, player.Credits, requiredCredits);
                        baseStateChanged = true;
                    }
                    else
                    {
                        _logger.LogDebug("Construction queue remains paused at Base {BaseId} (Credits: {Current}/{Required})", playerBase.Id, player.Credits, requiredCredits);
                    }
                    break; // Stop processing queue for this tick
                }
            } // End Pause/Start Check

            // --- Advance Timer (Only if not paused and item has time remaining) ---
            // Use RemainingBuildTimeSeconds directly, assuming it's initialized correctly above when starting.
            if (!playerBase.IsConstructionPaused && activeItem.RemainingBuildTimeSeconds > 0.001) // Use epsilon
            {
                double timeToComplete = activeItem.RemainingBuildTimeSeconds;
                double timeToApply = Math.Min(remainingSecondsInTick, timeToComplete);

                activeItem.RemainingBuildTimeSeconds -= timeToApply;
                remainingSecondsInTick -= timeToApply;
                baseStateChanged = true; // Queue item state changed

                // --- Check for Completion ---
                if (activeItem.RemainingBuildTimeSeconds <= 0.001) // Use epsilon
                {
                    _logger.LogInformation("Construction completed: {Structure} Lvl {Target} at Base {BaseId}", activeItem.StructureType, activeItem.TargetLevel, playerBase.Id);
                    playerBase.CompleteFirstQueueItem(); // Removes item, updates Base.Structures
                                                         // Loop will continue to next item if time remains in tick
                }
            }
            else if (!playerBase.IsConstructionPaused && playerBase.ConstructionQueue.Any() && activeItem.RemainingBuildTimeSeconds <= 0.001)
            {
                // Catch state where item completed but wasn't removed? Force removal.
                _logger.LogWarning("Queue item {Structure} Lvl {Target} had <= 0 time but wasn't removed? Forcing completion.", activeItem.StructureType, activeItem.TargetLevel);
                playerBase.CompleteFirstQueueItem();
                baseStateChanged = true;
            }
            // If paused, or no time left in tick, or queue became empty, loop terminates.

        } // End While Loop

        // Persist changes outside the loop if any occurred during the tick processing
        // Use Task.WhenAll for concurrent updates? Or keep sequential? Sequential safer for now.
        if (baseStateChanged) { await _baseRepository.UpdateAsync(playerBase); }
        if (playerStateChanged) { await _playerRepository.UpdateAsync(player); }
        // SaveChangesAsync is called after TickAsync in Program.cs
    }

    // Placeholder methods for queue pause state - needs implementation detail (e.g., bool flag on Base?)
    private bool IsQueuePaused(Base playerBase) => false; // TODO: Implement check
    private void SetQueuePaused(Base playerBase, bool isPaused) { /* TODO: Implement state change */ }
}


