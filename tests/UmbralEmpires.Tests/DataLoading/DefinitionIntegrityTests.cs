// tests/UmbralEmpires.Tests/DataLoading/DefinitionIntegrityTests.cs
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.TestDataBuilders;
using UmbralEmpires.Tests.Helpers; // Assuming CreateDefault... helpers might move here or be static

namespace UmbralEmpires.Tests.DataLoading;

public class DefinitionIntegrityTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // --- Helpers (Consider moving to TestHelpers if used across classes) ---
    private static TechnologyDefinition CreateValidTechnology(string id = "ValidTech", string name = "Valid Tech", int cost = 1, int labs = 1)
    {
        return new TechnologyDefinition { Id = id, Name = name, CreditsCost = cost, RequiredLabsLevel = labs, RequiresPrerequisites = new() };
    }

    private static UnitDefinition CreateValidUnit(string id = "ValidUnit", string name = "Valid Unit")
    {
        // Basic valid unit needing Stellar Drive for simplicity in these tests
        return new UnitDefinition
        {
            Id = id,
            Name = name,
            CreditsCost = 10,
            DriveType = "Stellar",
            WeaponType = "Laser",
            Attack = 1,
            Armour = 1,
            Shield = 0,
            Hangar = 0,
            Speed = 1,
            RequiredShipyard = new(1),
            // Add the mandatory "Stellar Drive" req due to current validator limitation
            RequiresTechnology = new List<TechRequirement> { new("Stellar Drive", 1) }
        };
    }

    private static StructureDefinition CreateValidStructure(string id = "ValidStruct", string name = "Valid Struct")
    {
        return new StructureDefinition { Id = id, Name = name, BaseCreditsCost = 10, RequiresTechnology = new() };
    }

    // --- Test Cases ---

    [Fact]
    public void Should_Skip_Unit_When_RequiredTechnology_Is_Missing()
    {
        // Arrange
        var unit = CreateValidUnit("TestUnit") with
        {
            // Add specific requirement besides the mandatory "Stellar Drive"
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1), new("Stellar Drive", 1) }
        };
        // DO NOT add "Laser" TechnologyDefinition

        var json = TestHelpers.CreateBuilder()
            .WithTechnology(CreateValidTechnology("Stellar Drive")) // Add mandatory tech
            .WithUnit(unit)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Units.Should().BeEmpty(); // Unit should be skipped
        result.Technologies.Should().ContainSingle(t => t.Id == "Stellar Drive"); // Ensure the existing tech loaded
    }

    [Fact]
    public void Should_Load_Unit_When_RequiredTechnology_Is_Present_And_Valid()
    {
        // Arrange
        var laserTech = CreateValidTechnology("Laser");
        var stellarTech = CreateValidTechnology("Stellar Drive");
        var unit = CreateValidUnit("TestUnit") with
        {
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1), new("Stellar Drive", 1) }
        };
        var expectedUnits = new List<UnitDefinition> { unit };

        var json = TestHelpers.CreateBuilder()
            .WithTechnology(laserTech)
            .WithTechnology(stellarTech)
            .WithUnit(unit)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Units.Should().BeEquivalentTo(expectedUnits); // Unit should load
        result.Technologies.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Skip_Unit_When_RequiredTechnology_Is_Present_But_Invalid()
    {
        // Arrange
        // Make Laser tech invalid
        var invalidLaserTech = CreateValidTechnology("Laser") with { CreditsCost = -100 };
        var stellarTech = CreateValidTechnology("Stellar Drive"); // This one is valid
        var unit = CreateValidUnit("TestUnit") with
        {
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1), new("Stellar Drive", 1) }
        };

        var json = TestHelpers.CreateBuilder()
            .WithTechnology(invalidLaserTech) // Add the invalid tech
            .WithTechnology(stellarTech)      // Add the valid stellar tech
            .WithUnit(unit)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Technologies.Should().ContainSingle(t => t.Id == "Stellar Drive"); // Only valid tech loads
        result.Technologies.Should().NotContain(t => t.Id == "Laser");
        result.Units.Should().BeEmpty(); // Unit should be skipped because "Laser" req failed
    }

    [Fact]
    public void Should_Load_Unit_When_RequiredTechnology_Case_Differs_But_Exists()
    {
        // Arrange
        var laserTech = CreateValidTechnology("Laser"); // Definition uses "Laser"
        var stellarTech = CreateValidTechnology("Stellar Drive");
        var unit = CreateValidUnit("TestUnit") with
        {
            // Requirement uses "laser" (lowercase)
            RequiresTechnology = new List<TechRequirement> { new("laser", 1), new("Stellar Drive", 1) }
        };
        var expectedUnits = new List<UnitDefinition> { unit };

        var json = TestHelpers.CreateBuilder()
            .WithTechnology(laserTech)
            .WithTechnology(stellarTech)
            .WithUnit(unit)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Units.Should().BeEquivalentTo(expectedUnits); // Unit should load if lookup is case-insensitive
        result.Technologies.Should().HaveCount(2);
    }

    [Fact]
    public void Should_Skip_Technology_When_Prerequisite_Is_Missing()
    {
        // Arrange
        var techB = CreateValidTechnology("TechB", labs: 2) with
        {
            RequiresPrerequisites = new List<TechRequirement> { new("TechA", 1) }
        };
        // DO NOT define TechA

        var json = TestHelpers.CreateBuilder()
            .WithTechnology(techB)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Technologies.Should().BeEmpty(); // TechB skipped due to missing prereq TechA
    }

    [Fact]
    public void Should_Skip_Structure_When_RequiredTechnology_Is_Missing()
    {
        // Arrange
        var structure = CreateValidStructure("TestStruct") with
        {
            RequiresTechnology = new List<TechRequirement> { new("MissingTech", 1) }
        };
        // DO NOT define MissingTech

        var json = TestHelpers.CreateBuilder()
            .WithStructure(structure)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Structures.Should().BeEmpty(); // Structure skipped
    }

    // Add similar tests for Defenses requiring missing/invalid tech
    // Add tests for Technologies requiring invalid prerequisites
}