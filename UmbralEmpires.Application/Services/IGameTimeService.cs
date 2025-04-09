namespace UmbralEmpires.Application.Services;

public interface IGameTimeService
{
    /// <summary>
    /// Processes one tick of game time, updating player resources and base activities.
    /// </summary>
    /// <param name="playerId">ID of the player to update.</param>
    /// <param name="elapsedRealTimeSeconds">Real-world seconds passed since the last tick.</param>
    /// <param name="gameHoursPerRealSecond">How many game hours pass per real-world second (controls game speed).</param>
    Task TickAsync(Guid playerId, double elapsedRealTimeSeconds, double gameHoursPerRealSecond = 1.0); // Default 1:1 game hour per real second
}