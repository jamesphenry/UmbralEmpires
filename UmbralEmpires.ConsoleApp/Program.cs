// Using statements for DI, Logging, EF Core, Services, Repos, Core types, UI
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using UmbralEmpires.Application.Persistence; // Repository Interfaces
using UmbralEmpires.Application.Services;    // Service Interfaces & Impls
using UmbralEmpires.Application.GameData;   // For StructureDataLookup (if static methods used directly)
using UmbralEmpires.Data;                    // DbContext
using UmbralEmpires.Data.Repositories;       // Repository Impls
using UmbralEmpires.Core.Gameplay;         // For Player, Base etc. (needed for setup/UI logic)
using UmbralEmpires.Core.World;            // For Astro etc. (needed for setup/UI logic)
using System;
using System.Threading.Tasks;                // For Task

// --- Dependency Injection Setup ---
var services = new ServiceCollection();

// 1. Logging
services.AddLogging(configure => {
    configure.AddConsole();
    // configure.SetMinimumLevel(LogLevel.Debug);
});

// 2. EF Core DbContext
services.AddDbContext<UmbralDbContext>(); // Scoped lifetime default

// 3. Repositories
services.AddScoped<IBaseRepository, BaseRepository>();
services.AddScoped<IAstroRepository, AstroRepository>();
services.AddScoped<IPlayerRepository, PlayerRepository>();
// TODO: Add IUnitOfWork

// 4. Application Services
services.AddScoped<IBaseStatsCalculatorService, BaseStatsCalculatorService>();
services.AddScoped<IConstructionService, ConstructionService>();
services.AddScoped<IGameTimeService, GameTimeService>();
// TODO: Add IGameSetupService

// Build the Service Provider
var serviceProvider = services.BuildServiceProvider(validateScopes: true);

// --- Application Initialization ---
Application.Init();

// --- Main Application Logic ---
try
{
    var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
    logger.LogInformation("Umbral Empires starting...");

    // --- TODO: Refine Initial Game State Setup ---
    Guid currentPlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // FIXED ID for M1
    using (var setupScope = serviceProvider.CreateScope())
    {
        var playerRepo = setupScope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var dbContext = setupScope.ServiceProvider.GetRequiredService<UmbralDbContext>();
        var player = await playerRepo.GetByIdAsync(currentPlayerId);
        if (player == null)
        {
            logger.LogInformation("Creating initial player state...");
            player = new Player(currentPlayerId, "Emperor");
            await playerRepo.AddAsync(player);
            // TODO: Create initial Astro and Base here too and save them
            await dbContext.SaveChangesAsync(); // Save initial state
        }
    }

    // --- UI Setup ---
    var top = Application.Top;

    var menu = new MenuBar(new MenuBarItem[] {
        new MenuBarItem ("_File", new MenuItem [] {
            new MenuItem ("_Quit", "", () => { Application.RequestStop(); }, shortcut: Key.Q | Key.CtrlMask)
        })
    });

    var mainWindow = new Window(" Umbral Empires ") { X = 0, Y = 1, Width = Dim.Fill(), Height = Dim.Fill(1), Border = new Border { BorderStyle = BorderStyle.None } };
    var statusBar = new StatusBar(new StatusItem[] {
        new StatusItem(Key.Q | Key.CtrlMask, "~^Q~ Quit", () => Application.RequestStop()),
        new StatusItem(Key.Null, "Status: Ready", null) // Status message label
    });

    var statusFrame = new FrameView("Base Status") { X = 0, Y = 0, Width = Dim.Percent(30), Height = Dim.Fill() };
    var queueFrame = new FrameView("Construction Queue") { X = Pos.Percent(70), Y = 0, Width = Dim.Percent(30), Height = Dim.Fill() };
    var structuresFrame = new FrameView("Structures") { X = Pos.Right(statusFrame), Y = 0, Width = Dim.Fill() - Dim.Width(statusFrame) - Dim.Width(queueFrame), Height = Dim.Fill() };

    // Add Placeholder Content (as before)
    statusFrame.Add(new Label(1, 1, "Coords: ?")); /* ... other labels ... */
    structuresFrame.Add(new Label(1, 1, "[Existing Structures List Here]")); /* ... etc ... */
    queueFrame.Add(new Label(1, 1, "[Queue List Here]"));

    mainWindow.Add(statusFrame, structuresFrame, queueFrame);
    top.Add(menu, mainWindow, statusBar);


    // --- Game Loop Timer ---
    TimeSpan tickInterval = TimeSpan.FromSeconds(1);
    double gameHoursPerRealSecond = 1.0;

    object? timerToken = null; // <<< CORRECTED TYPE HERE
    timerToken = Application.MainLoop.AddTimeout(tickInterval, (mainLoop) => { // Assign to corrected variable
        _ = Task.Run(async () => { // Run tick logic in background
            try
            {
                using (var scope = serviceProvider.CreateScope()) // Create scope per tick
                {
                    var scopedTimeService = scope.ServiceProvider.GetRequiredService<IGameTimeService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<UmbralDbContext>();

                    await scopedTimeService.TickAsync(currentPlayerId, tickInterval.TotalSeconds, gameHoursPerRealSecond);

                    // --- Save Changes ---
                    int changes = await dbContext.SaveChangesAsync();
                    if (changes > 0)
                    {
                        logger.LogDebug("Saved {Count} changes after game tick.", changes);
                        // Trigger UI refresh maybe? Needs thread safety
                        // Application.MainLoop.Invoke(() => { /* TODO: UI refresh logic */ });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during game tick processing for Player {PlayerId}", currentPlayerId);
                // Consider how to handle tick errors - stop timer? Show message?
                // Application.MainLoop.Invoke(() => { statusBar.Items[1].Title = $"Error: Tick failed!"; });
            }
        });
        return true; // Keep the timer running
    });

    // --- Run UI ---
    Application.Run(top); // Blocks until Application.RequestStop()

}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error during application run: {ex}");
}
finally
{
    // Ensure shutdown is always called
    Application.Shutdown();
    Console.ResetColor();
}