using Moq; // Using Moq for mocking dependencies
using UmbralEmpires.Application.Persistence; // Repository Interfaces
using UmbralEmpires.Application.Services;    // Service Interface & Impls
using UmbralEmpires.Application.GameData;   // For StructureDataLookup access
using UmbralEmpires.Core.Gameplay;         // For Base, Player, ConstructionQueueItem etc.
using UmbralEmpires.Core.World;            // For Astro, AstroCoordinates etc.
using Microsoft.Extensions.Logging.Abstractions; // For NullLogger
using System;
using System.Collections.Generic;           // For List<>
using System.Threading.Tasks;
using Xunit;

namespace UmbralEmpires.Tests.Application.Services; // Adjust namespace if needed

public class ConstructionServiceTests
{
    // --- Mocks for Dependencies ---
    private readonly Mock<IBaseRepository> _mockBaseRepository;
    private readonly Mock<IAstroRepository> _mockAstroRepository;
    private readonly Mock<IBaseStatsCalculatorService> _mockStatsCalculator;
    private readonly NullLogger<ConstructionService> _nullLogger; // Use NullLogger for tests

    // --- System Under Test ---
    private readonly ConstructionService _sut;

    // --- Test Data (Setup reusable test objects) ---
    private readonly Guid _playerId = Guid.NewGuid();
    private readonly Guid _baseId = Guid.NewGuid();
    private readonly Guid _astroId = Guid.NewGuid();
    private readonly Astro _testAstro;
    private readonly Base _testBase;
    private readonly CalculatedBaseStats _testStats;

    public ConstructionServiceTests()
    {
        // Create Mocks
        _mockBaseRepository = new Mock<IBaseRepository>();
        _mockAstroRepository = new Mock<IAstroRepository>();
        _mockStatsCalculator = new Mock<IBaseStatsCalculatorService>();
        _nullLogger = new NullLogger<ConstructionService>();

        // Setup default test data (initial state)
        var coords = new AstroCoordinates("T00", 1, 1, 1);
        _testAstro = new Astro(_astroId, coords, TerrainType.Earthly, true, 3, 3, 0, 4, 6, 85);
        _testBase = new Base(_baseId, _astroId, _playerId, "TestBase"); // Starts with Lvl 1 Urban
        _testAstro.AssignBase(_baseId); // Link them

        // Define the stats the mock calculator should return for this initial state
        _testStats = new CalculatedBaseStats(
            BaseEconomy: 0, EnergyProduction: 5, EnergyConsumption: 0,
            ConstructionCapacity: 15, ProductionCapacity: 0, ResearchCapacity: 0,
            MaxPopulation: 6, CurrentPopulationUsed: 1, MaxArea: 85, CurrentAreaUsed: 1);

        // --- Default Mock Setups ---
        // When GetByIdAsync is called with the specific IDs, return our test objects
        _mockBaseRepository.Setup(r => r.GetByIdAsync(_baseId)).ReturnsAsync(_testBase);
        _mockAstroRepository.Setup(r => r.GetByIdAsync(_astroId)).ReturnsAsync(_testAstro);
        // When CalculateStats is called for our base/astro (with Tech=0), return our predefined stats
        _mockStatsCalculator.Setup(s => s.CalculateStats(_testBase, _testAstro, 0, 0))
                              .Returns(_testStats);

        // Create the Service using the mocks
        _sut = new ConstructionService(
            _mockBaseRepository.Object, // Use .Object to get the mocked instance
            _mockAstroRepository.Object,
            _mockStatsCalculator.Object,
            _nullLogger
        );
    }

    [Fact]
    public async Task QueueStructureAsync_ValidRequest_AddsItemToQueueAndCallsUpdate()
    {
        // Arrange
        var structureToBuild = StructureType.MetalRefineries;
        int targetLevel = 1; // Current level is 0

        // Setup: Initial state (_testStats) allows building Metal Refinery Lvl 1:
        // EReq=0, PReq=1, AReq=1. Available: E=5, P=6-1=5, A=85-1=84. OK.
        // ConstructionCapacity = 15. Cost(L1)=1. Time=(1/15)*3600=240s.

        // Act: Call the service method under test
        var result = await _sut.QueueStructureAsync(_playerId, _baseId, structureToBuild);

        // Assert
        // 1. Check result indicates success
        Assert.True(result.Success, $"Expected success but got: {result.Message}");
        Assert.Contains("added to queue", result.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Check item was actually added to the Base object's queue list
        Assert.Single(_testBase.ConstructionQueue);
        var queuedItem = _testBase.ConstructionQueue[0];
        Assert.Equal(structureToBuild, queuedItem.StructureType);
        Assert.Equal(targetLevel, queuedItem.TargetLevel);

        // 3. Check calculated build time added to the queue item
        double expectedBuildTime = (double)StructureDataLookup.GetCreditCostForLevel(structureToBuild, targetLevel) / _testStats.ConstructionCapacity * 3600;
        Assert.Equal(expectedBuildTime, queuedItem.TotalBuildTimeSeconds);
        Assert.Equal(expectedBuildTime, queuedItem.RemainingBuildTimeSeconds); // Should be initialized to total

        // 4. Verify UpdateAsync was called on the base repository exactly once to save the queue change
        _mockBaseRepository.Verify(r => r.UpdateAsync(_testBase), Times.Once);
    }



    // --- TODO: Add tests for failure scenarios ---
    // [Fact] public async Task QueueStructureAsync_QueueFull_ReturnsFailure() { ... }
    // [Fact] public async Task QueueStructureAsync_InsufficientEnergy_ReturnsFailure() { ... }
    // [Fact] public async Task QueueStructureAsync_InsufficientPopulation_ReturnsFailure() { ... }
    // [Fact] public async Task QueueStructureAsync_InsufficientArea_ReturnsFailure() { ... }
    // [Fact] public async Task QueueStructureAsync_BaseNotFound_ReturnsFailure() { ... }
}