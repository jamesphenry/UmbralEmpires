// --- File: Program.cs ---

// Using statements (keep relevant ones)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using UmbralEmpires.Application.Persistence;
using UmbralEmpires.Application.Services;
using UmbralEmpires.Data;
using UmbralEmpires.Data.Repositories;
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using UmbralEmpires.ConsoleApp.UI; // Include the new UI namespace
using System;
using System.Linq;
using System.Threading.Tasks;

// --- Dependency Injection Setup ---
var services = new ServiceCollection();
services.AddLogging(configure => configure.AddConsole()); // Simplified
services.AddDbContext<UmbralDbContext>();
services.AddScoped<IBaseRepository, BaseRepository>();
services.AddScoped<IAstroRepository, AstroRepository>();
services.AddScoped<IPlayerRepository, PlayerRepository>();
services.AddScoped<IBaseStatsCalculatorService, BaseStatsCalculatorService>();
services.AddScoped<IConstructionService, ConstructionService>();
services.AddScoped<IGameTimeService, GameTimeService>();
// TODO: Add IGameSetupService registration

var serviceProvider = services.BuildServiceProvider(validateScopes: true);

// --- Application Initialization ---
Application.Init();

// --- Main Application Logic ---
ILogger? logger = null;
Guid currentPlayerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
Guid homeBaseId = Guid.Empty;

try
{
    logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
    logger.LogInformation("Umbral Empires starting...");

    // --- Ensure Initial Game State ---
    // This logic *should* move into an IGameSetupService eventually
    using (var setupScope = serviceProvider.CreateScope())
    {
        // (Same initial state check/creation logic as before...)
        // ... on completion, it sets homeBaseId ...
        var playerRepo = setupScope.ServiceProvider.GetRequiredService<IPlayerRepository>(); var baseRepo = setupScope.ServiceProvider.GetRequiredService<IBaseRepository>(); var astroRepo = setupScope.ServiceProvider.GetRequiredService<IAstroRepository>(); var dbContext = setupScope.ServiceProvider.GetRequiredService<UmbralDbContext>(); logger.LogInformation("Checking initial game state..."); var player = await playerRepo.GetByIdAsync(currentPlayerId); if (player == null) { player = new Player(currentPlayerId, "Emperor", 500); await playerRepo.AddAsync(player); logger.LogInformation("Created initial player."); }
        var homeBase = (await baseRepo.GetByPlayerIdAsync(currentPlayerId)).FirstOrDefault(); if (homeBase == null) { logger.LogInformation("Creating initial Astro and Base..."); var coords = new AstroCoordinates("U00", 50, 50, 10); var astro = new Astro(Guid.NewGuid(), coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85); await astroRepo.AddAsync(astro); homeBase = new Base(Guid.NewGuid(), astro.Id, player.Id, "Homeworld"); await baseRepo.AddAsync(homeBase); await dbContext.SaveChangesAsync(); astro.AssignBase(homeBase.Id); await astroRepo.UpdateAsync(astro); await dbContext.SaveChangesAsync(); logger.LogInformation("Initial state created."); } else { logger.LogInformation("Existing game state found."); }
        homeBaseId = homeBase.Id;
    }

    if (homeBaseId == Guid.Empty) { throw new Exception("Failed to find or create home base."); }

    // --- Setup Top Level UI Elements ---
    var menu = new MenuBar(new MenuBarItem[] { /* ... File->Quit ... */ });
    var statusMessageItem = new StatusItem(Key.Null, "Status: Initializing...", null);
    var statusBar = new StatusBar(new StatusItem[] { /* ... Quit shortcut, statusMessageItem ... */ }); // Create StatusBar

    // --- Create Main UI Window Instance ---
    // *** FIX 5: Pass the StatusBar instance ***
    var mainUI = new MainUserInterface(serviceProvider, currentPlayerId, homeBaseId, statusBar); // Pass statusBar

    // --- Add top-level elements ---
    Application.Top.Add(menu, mainUI, statusBar); // mainUI is the content window

    // --- Run Application ---
    Application.Run(Application.Top);

}
catch (Exception ex) { Console.Error.WriteLine($"Fatal error: {ex}"); logger?.LogCritical(ex, "Fatal error during application run."); }
finally { Application.Shutdown(); Console.ResetColor(); }