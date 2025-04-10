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
        _testAstro.AssignBase(_baseId);

        // Define the stats the mock calculator should return for this initial state
        _testStats = new CalculatedBaseStats(
            BaseEconomy: 0, EnergyProduction: 5, EnergyConsumption: 0,
            ConstructionCapacity: 15, ProductionCapacity: 0, ResearchCapacity: 0,
            MaxPopulation: 6, CurrentPopulationUsed: 1, MaxArea: 85, CurrentAreaUsed: 1);

        // --- Default Mock Setups ---
        // Setup repositories to return our test objects when requested
        _mockBaseRepository.Setup(r => r.GetByIdAsync(_baseId)).ReturnsAsync(_testBase);
        _mockAstroRepository.Setup(r => r.GetByIdAsync(_astroId)).ReturnsAsync(_testAstro);
        // Setup stats calculator to return consistent stats for validation checks within the service
        _mockStatsCalculator.Setup(s => s.CalculateStats(It.IsAny<Base>(), It.IsAny<Astro>(), It.IsAny<int>(), It.IsAny<int>()))
                              .Returns(_testStats);

        // Create the Service (System Under Test) using the mocks
        _sut = new ConstructionService(
            _mockBaseRepository.Object, // Use .Object to get the mocked instance
            _mockAstroRepository.Object,
            _mockStatsCalculator.Object,
            _nullLogger
        // Pass mock UnitOfWork later if needed
        );
    }

    [Fact]
    public async Task QueueStructureAsync_ValidRequest_AddsItemToQueueAndCallsUpdate()
    {
        // Arrange
        var structureToBuild = StructureType.MetalRefineries;
        int targetLevel = 1; // Current level is 0

        // Initial state setup in constructor provides _testBase with empty queue
        // and _mockStatsCalculator returning _testStats.
        // _testStats confirms limits are OK for Metal Refinery Lvl 1 (Req: E:0, P:1, A:1).

        // Act
        // *** CORRECTED: Call the method under test on the ConstructionService instance ***
        var result = await _sut.QueueStructureAsync(_playerId, _baseId, structureToBuild);

        // Assert
        // 1. Check result indicates success
        Assert.True(result.Success, $"QueueStructureAsync failed: {result.Message}");
        Assert.Contains("added to queue", result.Message, StringComparison.OrdinalIgnoreCase);

        // 2. Check item was actually added to the Base object's queue list
        Assert.Single(_testBase.ConstructionQueue); // Verify queue has exactly one item
        var queuedItem = _testBase.ConstructionQueue[0];
        Assert.Equal(structureToBuild, queuedItem.StructureType); // Verify correct type
        Assert.Equal(targetLevel, queuedItem.TargetLevel);        // Verify correct target level

        // 3. Check calculated build time added to the queue item
        // Formula: Cost(L1) / Capacity * 3600
        // Cost(L1) for MetalRefinery = 1 (from StructureDataLookup)
        // Capacity = 15 (from _testStats)
        double expectedBuildTime = (double)1 / 15 * 3600; // = 240 seconds
        Assert.Equal(expectedBuildTime, queuedItem.TotalBuildTimeSeconds);
        Assert.Equal(expectedBuildTime, queuedItem.RemainingBuildTimeSeconds); // Should be initialized to total

        // 4. Verify UpdateAsync was called on the base repository exactly once
        // This confirms the service is attempting to persist the change to the Base's queue
        _mockBaseRepository.Verify(r => r.UpdateAsync(_testBase), Times.Once);
    }

    [Fact]
    public async Task QueueStructureAsync_InsufficientEnergy_ReturnsFailure()
    {
        // Arrange
        var structureToBuild = StructureType.FusionPlants; // Fusion Plant requires 4 Energy per level
        int targetLevel = 1;

        // --- Modify the mocked stats to have insufficient energy ---
        var statsWithNoEnergyBuffer = _testStats with // Use 'with' expression to copy record
        {
            // Initial state: Prod=5, Cons=0. Available=5.
            // Fusion L1 requires 4. This *should* work based on current stats.
            // Let's make production equal consumption to simulate no buffer.
            EnergyProduction = 0, // Prod = Cons = 0 -> Available = 0
            EnergyConsumption = 0
            // Or alternatively, set consumption equal to production:
            // EnergyConsumption = _testStats.EnergyProduction // Prod=5, Cons=5 -> Available = 0
        };
        // Ensure stats calculator mock returns these modified stats
        _mockStatsCalculator.Setup(s => s.CalculateStats(_testBase, _testAstro, 0, 0))
                              .Returns(statsWithNoEnergyBuffer);

        // Act: Attempt to queue the Fusion Plant (requires 4 Energy)
        var result = await _sut.QueueStructureAsync(_playerId, _baseId, structureToBuild);

        // Assert
        Assert.False(result.Success); // Expect failure
        Assert.Contains("insufficient energy", result.Message, StringComparison.OrdinalIgnoreCase); // Check message

        // Verify UpdateAsync was NOT called
        _mockBaseRepository.Verify(r => r.UpdateAsync(It.IsAny<Base>()), Times.Never);
    }

    [Fact]
    public async Task QueueStructureAsync_InsufficientPopulation_ReturnsFailure()
    {
        // Arrange
        // Metal Refinery requires 1 Population per level
        var structureToBuild = StructureType.MetalRefineries;
        int targetLevel = 1; // Needs 1 pop

        // --- Modify the mocked stats to have insufficient population ---
        // Initial state: MaxPop=6, PopUsed=1. Available=5.
        // Simulate having no population headroom by setting MaxPop equal to current usage.
        var statsWithNoPopBuffer = _testStats with { MaxPopulation = _testStats.CurrentPopulationUsed }; // MaxPop = 1, PopUsed = 1 -> Available = 0
        _mockStatsCalculator.Setup(s => s.CalculateStats(_testBase, _testAstro, 0, 0))
                              .Returns(statsWithNoPopBuffer);

        // Act: Attempt to queue the Metal Refinery (requires 1 Population)
        var result = await _sut.QueueStructureAsync(_playerId, _baseId, structureToBuild);

        // Assert
        Assert.False(result.Success); // Expect failure
        Assert.Contains("insufficient population", result.Message, StringComparison.OrdinalIgnoreCase); // Check message

        // Verify UpdateAsync was NOT called
        _mockBaseRepository.Verify(r => r.UpdateAsync(It.IsAny<Base>()), Times.Never);
    }

    [Fact]
    public async Task QueueStructureAsync_InsufficientArea_ReturnsFailure()
    {
        // Arrange
        // Metal Refinery requires 1 Area per level (according to StructureDataLookup)
        var structureToBuild = StructureType.MetalRefineries;
        int targetLevel = 1; // Needs 1 Area

        // --- Modify the mocked stats to have insufficient area ---
        // Initial state has MaxArea=85, AreaUsed=1. Available=84.
        // Simulate zero available area by setting MaxArea = AreaUsed.
        var statsWithNoAreaBuffer = _testStats with { MaxArea = _testStats.CurrentAreaUsed }; // MaxArea = 1, AreaUsed = 1 -> Available = 0
        _mockStatsCalculator.Setup(s => s.CalculateStats(_testBase, _testAstro, 0, 0))
                              .Returns(statsWithNoAreaBuffer);

        // Act: Attempt to queue the Metal Refinery (requires 1 Area)
        var result = await _sut.QueueStructureAsync(_playerId, _baseId, structureToBuild);

        // Assert
        Assert.False(result.Success); // Expect failure
        Assert.Contains("insufficient area", result.Message, StringComparison.OrdinalIgnoreCase); // Check message

        // Verify UpdateAsync was NOT called
        _mockBaseRepository.Verify(r => r.UpdateAsync(It.IsAny<Base>()), Times.Never);
    }

}