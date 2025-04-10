// --- File: BaseStatsCalculatorServiceTests.cs (in UmbralEmpires.Tests project) ---
using UmbralEmpires.Application.Services;
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using System;
using Xunit; // Using xUnit

namespace UmbralEmpires.Tests.Application.Services;

public class BaseStatsCalculatorServiceTests
{
    private readonly BaseStatsCalculatorService _sut; // System Under Test

    public BaseStatsCalculatorServiceTests()
    {
        // The service currently has no injected dependencies (it uses the static StructureDataLookup)
        // If we injected dependencies later (like IStructureDataLookup), we'd mock them here using Moq.
        _sut = new BaseStatsCalculatorService();
    }

    [Fact] // Marks this as an xUnit test method
    public void CalculateStats_InitialState_ReturnsCorrectBaseValues()
    {
        // Arrange: Set up the initial game state according to GDD 4.4
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10); // Example coords
        // Earthly stats: Metal:3, Gas:3, Cry:0, Sol:4?, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        // Base constructor adds Lvl 1 Urban Structures automatically
        var initialBase = new Base(baseId, astroId, playerId, "Homeworld");

        // Define Expected results based on formulas and initial state (Lvl 1 Urban, Tech=0)
        // From GDD 4.3 and StructureDataLookup
        int expectedEconomy = 0;        // Lvl 1 Urban adds 0
        int expectedEnergyProd = 5;     // Base 5 + Lvl 1 Urban adds 0
        int expectedEnergyCons = 0;     // Lvl 1 Urban costs 0 E/lvl * 1 lvl = 0
        int expectedConstrCap = 15;     // Base 15 + Lvl 1 Urban adds 0; Cybernetics Lvl 0 -> Multiplier = 1
        int expectedProdCap = 0;        // Base 0 + Lvl 1 Urban adds 0; Cybernetics Lvl 0 -> Multiplier = 1
        int expectedResearchCap = 0;    // Base 0 + Lvl 1 Urban adds 0; AI Lvl 0 -> Multiplier = 1
        int expectedMaxPop = 1 * startingAstro.BaseFertility; // Lvl 1 Urban * Fertility (6) = 6
        int expectedPopUsed = 1 * 1;    // Lvl 1 Urban costs 1 P/lvl * 1 lvl = 1
        int expectedMaxArea = startingAstro.BaseArea; // Base Area (85) + Lvl 1 Urban adds 0 area mods = 85
        int expectedAreaUsed = 1 * 1;   // Lvl 1 Urban costs 1 A/lvl * 1 lvl = 1

        // Act: Run the calculation
        var result = _sut.CalculateStats(initialBase, startingAstro);

        // Assert: Verify each calculated value matches the expected value
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }

    [Fact]
    public void CalculateStats_WithOneRefinery_CalculatesCorrectly()
    {
        // Arrange ---
        // Create the same initial Astro and Base state
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10);
        // Earthly: Metal:3, Gas:3, Cry:0, Sol:4, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        var playerBase = new Base(baseId, astroId, playerId, "Homeworld"); // Starts with Lvl 1 Urban

        // *** Add a Level 1 Metal Refinery ***
        // We use the entity's method to simulate the state after building
        playerBase.SetStructureLevel(StructureType.MetalRefineries, 1);

        // Define Expected results (Initial State + Lvl 1 Refinery Effects)
        // Assuming Cybernetics/AI Level 0
        int expectedEconomy = 0 + 1; // Refinery adds +1 Economy/Level
        int expectedEnergyProd = 5;     // No change from refinery
        int expectedEnergyCons = 0 + 0; // Lvl 1 Urban E:0, Lvl 1 Refinery E:0
        int expectedConstrCap = 15 + (1 * startingAstro.MetalPotential); // Base(15) + Refinery(Lvl 1 * Metal 3) = 18
        int expectedProdCap = 0 + (1 * startingAstro.MetalPotential);    // Base(0) + Refinery(Lvl 1 * Metal 3) = 3
        int expectedResearchCap = 0;    // No change
        int expectedMaxPop = 6;         // No change from refinery
        int expectedPopUsed = 1 + 1;    // Lvl 1 Urban P:1 + Lvl 1 Refinery P:1 = 2
        int expectedMaxArea = 85;       // No change from refinery
        int expectedAreaUsed = 1 + 1;   // Lvl 1 Urban A:1 + Lvl 1 Refinery A:1 = 2

        // Act ---
        var result = _sut.CalculateStats(playerBase, startingAstro);

        // Assert ---
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }

    [Fact]
    public void CalculateStats_WithOneFusionPlant_CalculatesCorrectly()
    {
        // Arrange ---
        // Create the same initial Astro and Base state
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10);
        // Earthly: Metal:3, Gas:3, Cry:0, Sol:4, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        var playerBase = new Base(baseId, astroId, playerId, "Homeworld"); // Starts with Lvl 1 Urban

        // *** Add a Level 1 Fusion Plant ***
        playerBase.SetStructureLevel(StructureType.FusionPlants, 1);

        // Expected values (Initial State + Lvl 1 Fusion Plant Effects)
        // Based on StructureDataLookup: Fusion Plant (BC:20, E:4, P:0, A:1), produces 4 Energy/lvl.
        int expectedEconomy = 0;        // No change from Fusion Plant
        int expectedEnergyProd = 5 + 4; // Base(5) + Lvl 1 Fusion Plant(+4) = 9
        int expectedEnergyCons = 0 + 4; // Lvl 1 Urban E:0 + Lvl 1 Fusion Plant E:4 = 4
        int expectedConstrCap = 15;     // No change
        int expectedProdCap = 0;        // No change
        int expectedResearchCap = 0;    // No change
        int expectedMaxPop = 6;         // No change (Fusion Plant P:0)
        int expectedPopUsed = 1 + 0;    // Lvl 1 Urban P:1 + Lvl 1 Fusion Plant P:0 = 1
        int expectedMaxArea = 85;       // No change
        int expectedAreaUsed = 1 + 1;   // Lvl 1 Urban A:1 + Lvl 1 Fusion Plant A:1 = 2

        // Act ---
        var result = _sut.CalculateStats(playerBase, startingAstro);

        // Assert ---
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }

    [Fact]
    public void CalculateStats_WithRefineryAndFusionPlant_CalculatesCorrectly()
    {
        // Arrange ---
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10);
        // Earthly: Metal:3, Gas:3, Cry:0, Sol:4, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        var playerBase = new Base(baseId, astroId, playerId, "Homeworld"); // Starts with Lvl 1 Urban

        // Add structures
        playerBase.SetStructureLevel(StructureType.MetalRefineries, 1);
        playerBase.SetStructureLevel(StructureType.FusionPlants, 1);

        // Expected values (Initial + Lvl 1 Refinery + Lvl 1 Fusion Plant)
        // Urban:   E:0 P:1 A:1 | MaxPop = 1*6=6
        // Refinery:E:0 P:1 A:1 | Econ=1, Const=1*3=3, Prod=1*3=3
        // Fusion:  E:4 P:0 A:1 | E Prod=4
        int expectedEconomy = 0 + 1 + 0;         // Urban(0) + Refinery(1) + Fusion(0) = 1
        int expectedEnergyProd = 5 + 0 + 4;      // Base(5) + Refinery(0) + Fusion(4) = 9
        int expectedEnergyCons = 0 + 0 + 4;      // Urban(0) + Refinery(0) + Fusion(4) = 4
        int expectedConstrCap = 15 + 3 + 0;      // Base(15) + Refinery(3) + Fusion(0) = 18
        int expectedProdCap = 0 + 3 + 0;         // Base(0) + Refinery(3) + Fusion(0) = 3
        int expectedResearchCap = 0;             // No change
        int expectedMaxPop = 6;                  // Urban only
        int expectedPopUsed = 1 + 1 + 0;         // Urban(1) + Refinery(1) + Fusion(0) = 2
        int expectedMaxArea = 85;                // Base only
        int expectedAreaUsed = 1 + 1 + 1;        // Urban(1) + Refinery(1) + Fusion(1) = 3

        // Act ---
        var result = _sut.CalculateStats(playerBase, startingAstro);

        // Assert ---
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }

    [Fact]
    public void CalculateStats_WithLevel2Refinery_CalculatesCorrectly()
    {
        // Arrange ---
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10);
        // Earthly: Metal:3, Gas:3, Cry:0, Sol:4, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        var playerBase = new Base(baseId, astroId, playerId, "Homeworld"); // Starts with Lvl 1 Urban

        // *** Add a Level 2 Metal Refinery ***
        playerBase.SetStructureLevel(StructureType.MetalRefineries, 2);

        // Expected values (Initial + Lvl 2 Refinery Effects)
        // Urban:   E:0 P:1 A:1 | MaxPop = 1*6=6
        // Refinery L2: E:0*2=0, P:1*2=2, A:1*2=2 | Econ=1*2=2, Const=3*2=6, Prod=3*2=6
        int expectedEconomy = 0 + 2;        // Urban(0) + Refinery L2(2) = 2
        int expectedEnergyProd = 5;         // Base(5)
        int expectedEnergyCons = 0 + 0;     // Urban(0) + Refinery L2(0) = 0
        int expectedConstrCap = 15 + 6;     // Base(15) + Refinery L2(6) = 21
        int expectedProdCap = 0 + 6;        // Base(0) + Refinery L2(6) = 6
        int expectedResearchCap = 0;        // No change
        int expectedMaxPop = 6;             // Urban only
        int expectedPopUsed = 1 + 2;        // Urban(1) + Refinery L2(2) = 3
        int expectedMaxArea = 85;           // Base only
        int expectedAreaUsed = 1 + 2;       // Urban(1) + Refinery L2(2) = 3

        // Act ---
        var result = _sut.CalculateStats(playerBase, startingAstro);

        // Assert ---
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }

    [Fact]
    public void CalculateStats_WithRefineryAndRoboticFactory_CalculatesCorrectly()
    {
        // Arrange ---
        var astroId = Guid.NewGuid();
        var coords = new AstroCoordinates("U00", 50, 50, 10);
        // Earthly: Metal:3, Gas:3, Cry:0, Sol:4, Fert:6, Area:85
        var startingAstro = new Astro(astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);

        var playerId = Guid.NewGuid();
        var baseId = Guid.NewGuid();
        var playerBase = new Base(baseId, astroId, playerId, "Homeworld"); // Starts with Lvl 1 Urban

        // Add structures
        playerBase.SetStructureLevel(StructureType.MetalRefineries, 1);
        playerBase.SetStructureLevel(StructureType.RoboticFactories, 1);

        // Expected values (Initial + Lvl 1 Refinery + Lvl 1 Robotic Factory)
        // Urban:   E:0 P:1 A:1 | MaxPop = 1*6=6
        // Refinery:E:0 P:1 A:1 | Econ=1, Const=1*3=3, Prod=1*3=3
        // RoboFact:E:0 P:1 A:1 | Econ=1, Const=1*2=2, Prod=1*2=2
        int expectedEconomy = 0 + 1 + 1;         // Urban(0) + Refinery(1) + Robo(1) = 2
        int expectedEnergyProd = 5;              // Base(5)
        int expectedEnergyCons = 0 + 0 + 0;      // Urban(0) + Refinery(0) + Robo(0) = 0
        int expectedConstrCap = 15 + 3 + 2;      // Base(15) + Refinery(3) + Robo(2) = 20
        int expectedProdCap = 0 + 3 + 2;         // Base(0) + Refinery(3) + Robo(2) = 5
        int expectedResearchCap = 0;             // No change
        int expectedMaxPop = 6;                  // Urban only
        int expectedPopUsed = 1 + 1 + 1;         // Urban(1) + Refinery(1) + Robo(1) = 3
        int expectedMaxArea = 85;                // Base only
        int expectedAreaUsed = 1 + 1 + 1;        // Urban(1) + Refinery(1) + Robo(1) = 3

        // Act ---
        var result = _sut.CalculateStats(playerBase, startingAstro);

        // Assert ---
        Assert.Equal(expectedEconomy, result.BaseEconomy);
        Assert.Equal(expectedEnergyProd, result.EnergyProduction);
        Assert.Equal(expectedEnergyCons, result.EnergyConsumption);
        Assert.Equal(expectedConstrCap, result.ConstructionCapacity);
        Assert.Equal(expectedProdCap, result.ProductionCapacity);
        Assert.Equal(expectedResearchCap, result.ResearchCapacity);
        Assert.Equal(expectedMaxPop, result.MaxPopulation);
        Assert.Equal(expectedPopUsed, result.CurrentPopulationUsed);
        Assert.Equal(expectedMaxArea, result.MaxArea);
        Assert.Equal(expectedAreaUsed, result.CurrentAreaUsed);
    }
}