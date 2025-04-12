// tests/UmbralEmpires.Tests/DataLoading/StructureDefinitionLoadingTests.cs
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces; // Assuming interface is here
using UmbralEmpires.Infrastructure.DataLoading; // Assuming implementation is here
using UmbralEmpires.Tests.TestDataBuilders; // Assuming builder is here
using UmbralEmpires.Tests.Helpers; // Using static helpers

namespace UmbralEmpires.Tests.DataLoading;

public class StructureDefinitionLoadingTests
{
    // Instantiate loader for tests in this class
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // Helper to create a default valid structure for tests
    // Ensures all known properties have a default value for BeEquivalentTo comparisons
    private StructureDefinition CreateDefaultValidStructure(string id = "ValidStruct", string name = "Valid Name", int cost = 10)
    {
        return new StructureDefinition
        {
            Id = id,
            Name = name,
            BaseCreditsCost = cost,
            EnergyRequirementPerLevel = 0,
            PopulationRequirementPerLevel = 0,
            AreaRequirementPerLevel = 0,
            RequiresTechnology = new List<TechRequirement>(),
            EconomyBonus = 0,
            IsAdvanced = false,
            BaseConstructionBonus = 0,
            BaseProductionBonus = 0,
            BaseResearchBonus = 0,
            UsesMetal = false,
            UsesGas = false,
            UsesCrystal = false,
            UsesSolar = false,
            AddsPopCapacityByFertility = false,
            AreaCapacityBonus = 0,
            IncreasesAstroFertility = false
        };
    }

    // --- Basic Loading & Validation Tests ---

    [Fact]
    public void LoadAllDefinitions_Should_Load_Single_Simple_Structure()
    {
        // Arrange
        var expected = CreateDefaultValidStructure(id: "S1", name: "N1", cost: 1);
        var builder = TestHelpers.CreateBuilder(); // Use helper for builder

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<StructureDefinition>(
            _loader, builder, expected,
            b => b.WithStructure(expected), r => r.Structures
        );
    }

    [Fact]
    public void LoadAllDefinitions_Should_Load_Multiple_Simple_Structures()
    {
        // Arrange
        var structure1 = CreateDefaultValidStructure(id: "S1", name: "Struct 1", cost: 1);
        var structure2 = CreateDefaultValidStructure(id: "S2", name: "Struct 2", cost: 2);
        var expectedStructures = new List<StructureDefinition> { structure1, structure2 };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithStructure(structure1)
            .WithStructure(structure2)
            .BuildJson();

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Structures.Should().NotBeNull();
        result.Structures.Should().HaveCount(2);
        result.Structures.Should().BeEquivalentTo(expectedStructures);
    }

    [Fact]
    public void LoadAllDefinitions_Should_Skip_Object_With_Missing_Id()
    {
        // Arrange
        var invalidStructure = CreateDefaultValidStructure() with { Id = "" }; // Explicitly empty Id
        var validStructure = CreateDefaultValidStructure(id: "Valid");
        var expectedStructures = new List<StructureDefinition> { validStructure }; // Only expect the valid one

        var jsonInput = TestHelpers.CreateBuilder()
            .WithStructure(invalidStructure)
            .WithStructure(validStructure)
            .BuildJson();

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Structures.Should().NotBeNull();
        result.Structures.Should().BeEquivalentTo(expectedStructures); // Implicit count check
    }

    [Fact]
    public void LoadAllDefinitions_Should_Skip_Object_With_Negative_Cost()
    {
        // Arrange
        var invalidStructure = CreateDefaultValidStructure(id: "NegCost", cost: -10);
        var validStructure = CreateDefaultValidStructure(id: "PosCost", cost: 10);
        var expectedStructures = new List<StructureDefinition> { validStructure };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithStructure(invalidStructure)
            .WithStructure(validStructure)
            .BuildJson();

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Structures.Should().NotBeNull();
        result.Structures.Should().BeEquivalentTo(expectedStructures);
    }

    [Fact]
    public void LoadAllDefinitions_Should_Skip_Object_With_Missing_Name()
    {
        // Arrange
        var invalidStructure = CreateDefaultValidStructure() with { Name = "" }; // Explicitly empty Name
        var validStructure = CreateDefaultValidStructure(id: "ValidName", name: "Val");
        var expectedStructures = new List<StructureDefinition> { validStructure };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithStructure(invalidStructure)
            .WithStructure(validStructure)
            .BuildJson();

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Structures.Should().NotBeNull();
        result.Structures.Should().BeEquivalentTo(expectedStructures);
    }

    // --- Individual Property Loading Tests using the Static Helper ---

    [Fact] public void LoadAllDefinitions_Should_Load_EnergyRequirementPerLevel() => TestSingleProperty(CreateDefaultValidStructure("EnergyTest") with { EnergyRequirementPerLevel = 4 });
    [Fact] public void LoadAllDefinitions_Should_Load_PopulationRequirementPerLevel() => TestSingleProperty(CreateDefaultValidStructure("PopTest") with { PopulationRequirementPerLevel = 1 });
    [Fact] public void LoadAllDefinitions_Should_Load_AreaRequirementPerLevel() => TestSingleProperty(CreateDefaultValidStructure("AreaTest") with { AreaRequirementPerLevel = 1 });
    [Fact] public void LoadAllDefinitions_Should_Load_EconomyBonus() => TestSingleProperty(CreateDefaultValidStructure("EconTest") with { EconomyBonus = 5 });
    [Fact] public void LoadAllDefinitions_Should_Load_IsAdvanced_Flag() => TestSingleProperty(CreateDefaultValidStructure("AdvTest") with { IsAdvanced = true });
    [Fact] public void LoadAllDefinitions_Should_Load_BaseConstructionBonus() => TestSingleProperty(CreateDefaultValidStructure("ConstTest") with { BaseConstructionBonus = 2 });
    [Fact] public void LoadAllDefinitions_Should_Load_BaseProductionBonus() => TestSingleProperty(CreateDefaultValidStructure("ProdTest") with { BaseProductionBonus = 3 });
    [Fact] public void LoadAllDefinitions_Should_Load_BaseResearchBonus() => TestSingleProperty(CreateDefaultValidStructure("ResTest") with { BaseResearchBonus = 8 });
    [Fact] public void LoadAllDefinitions_Should_Load_UsesMetal_Flag() => TestSingleProperty(CreateDefaultValidStructure("MetalTest") with { UsesMetal = true });
    [Fact] public void LoadAllDefinitions_Should_Load_UsesGas_Flag() => TestSingleProperty(CreateDefaultValidStructure("GasTest") with { UsesGas = true });
    [Fact] public void LoadAllDefinitions_Should_Load_UsesCrystal_Flag() => TestSingleProperty(CreateDefaultValidStructure("CrystalTest") with { UsesCrystal = true });
    [Fact] public void LoadAllDefinitions_Should_Load_UsesSolar_Flag() => TestSingleProperty(CreateDefaultValidStructure("SolarTest") with { UsesSolar = true });
    [Fact] public void LoadAllDefinitions_Should_Load_AddsPopCapacityByFertility_Flag() => TestSingleProperty(CreateDefaultValidStructure("PopFertTest") with { AddsPopCapacityByFertility = true });
    [Fact] public void LoadAllDefinitions_Should_Load_AreaCapacityBonus() => TestSingleProperty(CreateDefaultValidStructure("AreaBonusTest") with { AreaCapacityBonus = 10 });
    [Fact] public void LoadAllDefinitions_Should_Load_IncreasesAstroFertility_Flag() => TestSingleProperty(CreateDefaultValidStructure("FertIncTest") with { IncreasesAstroFertility = true });

    [Fact]
    public void LoadAllDefinitions_Should_Load_RequiresTechnology_List()
    {
        var expectedReq = new List<TechRequirement> { new("TestTech", 1) };
        var expected = CreateDefaultValidStructure("TechReqTest") with { RequiresTechnology = expectedReq };
        TestSingleProperty(expected);
    }

    // Private helper using the static helper for structure-specific tests
    private void TestSingleProperty(StructureDefinition expectedDefinition)
    {
        TestHelpers.TestSingleDefinitionProperty<StructureDefinition>(
             _loader, TestHelpers.CreateBuilder(), expectedDefinition,
             b => b.WithStructure(expectedDefinition), r => r.Structures
         );
    }
}