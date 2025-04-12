using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.TestDataBuilders;
using UmbralEmpires.Tests.Helpers;

namespace UmbralEmpires.Tests.DataLoading;

public class UnitDefinitionLoadingTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // Helper to create a default valid unit - NOTE: Does NOT set DriveType/WeaponType
    private UnitDefinition CreateDefaultValidUnit(string id = "ValidUnit", string name = "Valid Unit Name", int cost = 1)
    {
        // DriveType/WeaponType default to string.Empty based on record definition
        return new UnitDefinition { Id = id, Name = name, CreditsCost = cost };
    }

    // --- Test Cases ---
    [Fact]
    public void Should_Load_Single_Simple_Unit()
    {
        // Arrange -----
        var expected = CreateDefaultValidUnit(id: "Fighter", name: "Fighters", cost: 5) with
        {
            DriveType = "Inter", // Use correct "Inter"
            WeaponType = "Laser",
            Attack = 2,
            Armour = 2,
            Shield = 0,
            Hangar = 0,
            Speed = 1,
            RequiredShipyard = new ShipyardRequirement(1),
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1) }
        };

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<UnitDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithUnit(expected),
            r => r.Units
        );

        // Additionally verify other lists are empty
        var builder = TestHelpers.CreateBuilder().WithUnit(expected);
        var jsonInput = builder.BuildJson();
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        result.Structures.Should().BeEmpty();
        result.Technologies.Should().BeEmpty();
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Cost()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "NegCostUnit", cost: -100) with { DriveType = "Inter", WeaponType = "Laser" }; // Need valid Drive/Weapon
        var validUnit = CreateDefaultValidUnit(id: "PosCostUnit", cost: 100) with { DriveType = "Inter", WeaponType = "Laser" }; // Need valid Drive/Weapon
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_Id()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(name: "No ID") with { Id = "", DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidUnitId", name: "Valid Unit") with { DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_Name()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidNameUnit") with { Name = "", DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidNameUnit", name: "Valid Name") with { DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Attack()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidAttackUnit") with { Attack = -5, DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidAttackUnit", name: "Valid Attack") with { Attack = 5, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Armour()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidArmourUnit") with { Armour = -5, DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidArmourUnit", name: "Valid Armour") with { Armour = 5, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Shield()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidShieldUnit") with { Shield = -5, DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidShieldUnit", name: "Valid Shield") with { Shield = 5, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Hangar()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidHangarUnit") with { Hangar = -5, DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidHangarUnit", name: "Valid Hangar") with { Hangar = 5, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Speed()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSpeedUnit") with { Speed = -5, DriveType = "Inter", WeaponType = "Laser" };
        var validUnit = CreateDefaultValidUnit(id: "ValidSpeedUnit", name: "Valid Speed") with { Speed = 5, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_RequiredShipyard_BaseLevel()
    {
        // Arrange -----
        var invalidRequirement = new ShipyardRequirement(BaseLevel: -1, OrbitalLevel: 0);
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSYBaseUnit") with { RequiredShipyard = invalidRequirement, DriveType = "Inter", WeaponType = "Laser" };
        var validRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: 0);
        var validUnit = CreateDefaultValidUnit(id: "ValidSYBaseUnit", name: "Valid SY Base") with { RequiredShipyard = validRequirement, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_RequiredShipyard_OrbitalLevel()
    {
        // Arrange -----
        var invalidRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: -1);
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSYOrbitalUnit") with { RequiredShipyard = invalidRequirement, DriveType = "Inter", WeaponType = "Laser" };
        var validRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: 0);
        var validUnit = CreateDefaultValidUnit(id: "ValidSYOrbitalUnit", name: "Valid SY Orbital") with { RequiredShipyard = validRequirement, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Invalid_RequiredTechnology_TechId()
    {
        // Arrange -----
        var invalidReqs = new List<TechRequirement> { new("", 1) };
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidReqTechIdUnit") with { RequiresTechnology = invalidReqs, DriveType = "Inter", WeaponType = "Laser" };

        var validReqs = new List<TechRequirement> { new("ValidTech", 1) };
        var validUnit = CreateDefaultValidUnit(id: "ValidReqTechIdUnit", name: "Valid Req TechId") with { RequiresTechnology = validReqs, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Invalid_RequiredTechnology_Level()
    {
        // Arrange -----
        var invalidReqs = new List<TechRequirement> { new("SomeTech", 0) };
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidReqLevelUnit") with { RequiresTechnology = invalidReqs, DriveType = "Inter", WeaponType = "Laser" };

        var validReqs = new List<TechRequirement> { new("SomeTech", 1) };
        var validUnit = CreateDefaultValidUnit(id: "ValidReqLevelUnit", name: "Valid Req Level") with { RequiresTechnology = validReqs, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Duplicate_RequiredTechnology_TechIds()
    {
        // Arrange -----
        var invalidReqs = new List<TechRequirement> { new("DupTech", 1), new("DupTech", 2) };
        var invalidUnit = CreateDefaultValidUnit(id: "DuplicateReqUnit") with { RequiresTechnology = invalidReqs, DriveType = "Inter", WeaponType = "Laser" };

        var validReqs = new List<TechRequirement> { new("Tech1", 1), new("Tech2", 1) };
        var validUnit = CreateDefaultValidUnit(id: "NonDuplicateReqUnit", name: "Non-Duplicate Reqs") with { RequiresTechnology = validReqs, DriveType = "Inter", WeaponType = "Laser" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_DriveType()
    {
        // Arrange -----
        // Invalid unit has empty DriveType, but ensure WeaponType is set
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidDriveUnit") with { DriveType = "", WeaponType = "Laser" };

        // Valid unit must have valid DriveType, WeaponType, AND satisfy Drive/Tech rules
        var validReqs = new List<TechRequirement> { new("Stellar Drive", 1) }; // <<< ADD REQUIRED TECH
        var validUnit = CreateDefaultValidUnit(id: "ValidDriveUnit", name: "Valid Drive") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = validReqs // <<< ASSIGN REQUIRED TECH
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_WeaponType()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidWeaponUnit") with { DriveType = "Stellar", WeaponType = "" };
        var validReqs = new List<TechRequirement> { new("Stellar Drive", 1) };
        var validUnit = CreateDefaultValidUnit(id: "ValidWeaponUnit", name: "Valid Weapon") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = validReqs
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Null_Entry_In_RequiresTechnology()
    {
        // Arrange -----
        var jsonInput = """
        {
          "Structures": [],
          "Technologies": [],
          "Units": [
            { 
              "Id": "UnitWithNullReq", 
              "Name": "N", 
              "CreditsCost": 1, 
              "Attack": 1, 
              "Armour": 1, 
              "Shield": 0, 
              "Hangar": 0, 
              "Speed": 1, 
              "DriveType": "Stellar", 
              "WeaponType": "Laser", 
              "RequiredShipyard": { "BaseLevel": 1, "OrbitalLevel": 0 },
              "RequiresTechnology": [ null ] 
            },
            { 
              "Id": "ValidUnitAlongsideNull", 
              "Name": "V", 
              "CreditsCost": 1, 
              "Attack": 1, 
              "Armour": 1, 
              "Shield": 0, 
              "Hangar": 0, 
              "Speed": 1, 
              "DriveType": "Stellar", 
              "WeaponType": "Laser",
              "RequiredShipyard": { "BaseLevel": 1, "OrbitalLevel": 0 },
              "RequiresTechnology": [ { "TechId": "Stellar Drive", "Level": 1 } ] 
            }
          ]
        }
        """;

        var validUnit = CreateDefaultValidUnit(id: "ValidUnitAlongsideNull", name: "V") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            Attack = 1,
            Armour = 1,
            Speed = 1,
            RequiresTechnology = new List<TechRequirement> { new("Stellar Drive", 1) } // Needs tech for Stellar Drive
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Load_Unit_Description()
    {
        // Arrange -----
        var expectedDescription = "This is a test unit description.";
        var requiredTechs = new List<TechRequirement> { new("Stellar Drive", 1) };
        var expectedUnit = CreateDefaultValidUnit(id: "DescUnit") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = requiredTechs,
            Description = expectedDescription
        };
        var expectedUnits = new List<UnitDefinition> { expectedUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(expectedUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().ContainSingle()
            .Which.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void Should_Skip_Unit_With_Unknown_DriveType()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "UnknownDriveUnit") with
        {
            DriveType = "Hyperdrive",
            WeaponType = "Laser"
        };

        // Valid unit needs tech if DriveType is Stellar/Warp
        var validReqs = new List<TechRequirement> { new("Stellar Drive", 1) };
        var validUnit = CreateDefaultValidUnit(id: "KnownDriveUnit", name: "Known Drive") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = validReqs
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Stellar_Unit_Without_StellarDrive_Tech()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "MissingStellarDriveReq") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = new List<TechRequirement>()
        };

        var validReqs = new List<TechRequirement> { new("Stellar Drive", 1) };
        var validUnit = CreateDefaultValidUnit(id: "HasStellarDriveReq", name: "Valid Stellar") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            RequiresTechnology = validReqs
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Warp_Unit_Without_WarpDrive_Tech()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "MissingWarpDriveReq") with
        {
            DriveType = "Warp",
            WeaponType = "Laser",
            RequiresTechnology = new List<TechRequirement>()
        };

        var validReqs = new List<TechRequirement> { new("Warp Drive", 1) };
        var validUnit = CreateDefaultValidUnit(id: "HasWarpDriveReq", name: "Valid Warp") with
        {
            DriveType = "Warp",
            WeaponType = "Laser",
            RequiresTechnology = validReqs
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }
}