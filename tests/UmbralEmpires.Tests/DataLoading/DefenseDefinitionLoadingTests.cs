// tests/UmbralEmpires.Tests/DataLoading/DefenseDefinitionLoadingTests.cs
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.TestDataBuilders; // Using builder
using UmbralEmpires.Tests.Helpers;

namespace UmbralEmpires.Tests.DataLoading;

public class DefenseDefinitionLoadingTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // Helper to create a default valid defense for tests
    private DefenseDefinition CreateDefaultValidDefense(
        string id = "ValidDefense",
        string name = "Valid Name",
        int cost = 10,
        string weapon = "Laser")
    {
        return new DefenseDefinition
        {
            Id = id,
            Name = name,
            BaseCreditsCost = cost,
            WeaponType = weapon,
            Attack = 1,
            Armour = 1,
            Shield = 0,
            EnergyRequirementPerLevel = 0,
            PopulationRequirementPerLevel = 1,
            AreaRequirementPerLevel = 1,
            RequiresTechnology = new List<TechRequirement>()
        };
    }

    [Fact]
    public void Should_Load_Single_Simple_Defense()
    {
        // Arrange
        var expected = CreateDefaultValidDefense(id: "Barracks", name: "Barracks", cost: 5, weapon: "Laser") with
        {
            Attack = 4,
            Armour = 4,
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1) }
        };

        // Act & Assert using static helper (this one was already using the builder correctly via helper)
        TestHelpers.TestSingleDefinitionProperty<DefenseDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithDefense(expected), // Builder setup via Func
            r => r.Defenses
        );

        // Verify other lists are empty (also uses builder)
        var builder = TestHelpers.CreateBuilder().WithDefense(expected);
        var jsonInput = builder.BuildJson();
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        result.Structures.Should().BeEmpty();
        result.Technologies.Should().BeEmpty();
        result.Units.Should().BeEmpty();
    }

    [Fact]
    public void Should_Load_Multiple_Simple_Defenses()
    {
        // Arrange
        var defense1 = CreateDefaultValidDefense(id: "D1", name: "Defense 1", cost: 1);
        var defense2 = CreateDefaultValidDefense(id: "D2", name: "Defense 2", cost: 2);
        var expectedDefenses = new List<DefenseDefinition> { defense1, defense2 };

        // *** Use builder for setup ***
        var jsonInput = TestHelpers.CreateBuilder()
            .WithDefense(defense1)
            .WithDefense(defense2)
            .BuildJson();

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Defenses.Should().NotBeNull();
        result.Defenses.Should().BeEquivalentTo(expectedDefenses);
    }

    [Fact]
    public void Should_Skip_Defense_With_Missing_Id()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(name: "No ID Defense", weapon: "Laser") with { Id = "" };
        var valid = CreateDefaultValidDefense(id: "ValidIDDefense");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Missing_Name()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NoNameDefense", weapon: "Laser") with { Name = "" };
        var valid = CreateDefaultValidDefense(id: "ValidNameDefense");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_Cost()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegCostDefense", cost: -10);
        var valid = CreateDefaultValidDefense(id: "PosCostDefense");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Missing_WeaponType()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NoWeaponDefense") with { WeaponType = "" };
        var valid = CreateDefaultValidDefense(id: "ValidWeaponDefense", weapon: "Laser");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_Attack()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegAttack") with { Attack = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidAttack");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_Armour()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegArmour") with { Armour = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidArmour");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_Shield()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegShield") with { Shield = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidShield");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_EnergyRequirementPerLevel()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegEnergy") with { EnergyRequirementPerLevel = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidEnergy");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_PopulationRequirementPerLevel()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegPop") with { PopulationRequirementPerLevel = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidPop");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Negative_AreaRequirementPerLevel()
    {
        // Arrange
        var invalid = CreateDefaultValidDefense(id: "NegArea") with { AreaRequirementPerLevel = -1 };
        var valid = CreateDefaultValidDefense(id: "ValidArea");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Invalid_RequiredTechnology_TechId()
    {
        // Arrange
        var invalidReqs = new List<TechRequirement> { new("", 1) };
        var invalid = CreateDefaultValidDefense(id: "InvReqId") with { RequiresTechnology = invalidReqs };
        var valid = CreateDefaultValidDefense(id: "ValReqId");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Invalid_RequiredTechnology_Level()
    {
        // Arrange
        var invalidReqs = new List<TechRequirement> { new("Tech", 0) };
        var invalid = CreateDefaultValidDefense(id: "InvReqLvl") with { RequiresTechnology = invalidReqs };
        var valid = CreateDefaultValidDefense(id: "ValReqLvl");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Duplicate_RequiredTechnology_TechIds()
    {
        // Arrange
        var invalidReqs = new List<TechRequirement> { new("Dup", 1), new("Dup", 2) };
        var invalid = CreateDefaultValidDefense(id: "DupReq") with { RequiresTechnology = invalidReqs };
        var valid = CreateDefaultValidDefense(id: "NoDupReq");
        var expected = new List<DefenseDefinition> { valid };

        // *** Use builder for setup ***
        var json = TestHelpers.CreateBuilder()
            .WithDefense(invalid)
            .WithDefense(valid)
            .BuildJson();

        // Act
        var result = _loader.LoadAllDefinitions(json);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Skip_Defense_With_Null_Entry_In_RequiresTechnology() // Manual JSON needed for null
    {
        // Arrange
        var jsonInput = """
        {
          "Structures": [], "Technologies": [], "Units": [],
          "Defenses": [
            { "Id": "DefWithNullReq", "Name": "D", "BaseCreditsCost": 1, "WeaponType": "Laser", "Attack": 1, "Armour": 1, "Shield": 0, "EnergyRequirementPerLevel": 0, "PopulationRequirementPerLevel": 1, "AreaRequirementPerLevel": 1, "RequiresTechnology": [ null ] },
            { "Id": "ValidDefAlongNull", "Name": "V", "BaseCreditsCost": 1, "WeaponType": "Laser", "Attack": 1, "Armour": 1, "Shield": 0, "EnergyRequirementPerLevel": 0, "PopulationRequirementPerLevel": 1, "AreaRequirementPerLevel": 1, "RequiresTechnology": [] }
          ]
        }
        """;
        // Use helper to create expected valid object for comparison
        var validDef = CreateDefaultValidDefense(id: "ValidDefAlongNull", name: "V", cost: 1, weapon: "Laser");
        var expected = new List<DefenseDefinition> { validDef };

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Defenses.Should().BeEquivalentTo(expected);
    }
}