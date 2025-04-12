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

    // Helper to create a default valid unit (adjust defaults as needed)
    private UnitDefinition CreateDefaultValidUnit(string id = "ValidUnit", string name = "Valid Unit Name", int cost = 1)
    {
        return new UnitDefinition { Id = id, Name = name, CreditsCost = cost };
    }

    // --- NEW TEST ---
    [Fact]
    public void Should_Load_Single_Simple_Unit()
    {
        // Arrange -----
        // Using Fighter unit as example
        var expected = CreateDefaultValidUnit(id: "Fighter", name: "Fighters", cost: 5) with
        {
            DriveType = "Interceptor",
            WeaponType = "Laser",
            Attack = 2,
            Armour = 2,
            Shield = 0,
            Hangar = 0,
            Speed = 1,
            RequiredShipyard = new ShipyardRequirement(1),
            RequiresTechnology = new List<TechRequirement> { new("Laser", 1) }
            // Only includes properties defined in UnitDefinition record so far
        };

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<UnitDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithUnit(expected), // Use WithUnit
            r => r.Units               // Select Units list
        );

        // Additionally verify other lists are empty
        var builder = TestHelpers.CreateBuilder().WithUnit(expected);
        var jsonInput = builder.BuildJson();
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        result.Structures.Should().BeEmpty();
        result.Technologies.Should().BeEmpty();

        // --- TEMPORARY Assert if needed ---
        // Assert.True(false, "Implement Unit loading.");
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Cost()
    {
        // Arrange -----
        var invalidUnit = CreateDefaultValidUnit(id: "NegCostUnit", cost: -100);
        var validUnit = CreateDefaultValidUnit(id: "PosCostUnit", cost: 100);
        var expectedUnits = new List<UnitDefinition> { validUnit }; // Only expect the valid one

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits); // Implicitly checks count and content

        // --- TEMPORARY Assert if needed ---
        // Assert.True(false, "Verify implementation skips units with negative cost.");
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_Id()
    {
        // Arrange -----
        // Create an invalid unit with an empty Id
        var invalidUnit = CreateDefaultValidUnit() with { Id = "" }; // Explicitly empty Id
        // Create a valid unit to ensure it's still loaded
        var validUnit = CreateDefaultValidUnit(id: "ValidUnitId", name: "Valid Unit");
        var expectedUnits = new List<UnitDefinition> { validUnit }; // Only expect the valid one

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit) // Add the invalid one
            .WithUnit(validUnit)   // Add the valid one
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering()); // Ensure only the valid one is present
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_Name()
    {
        // Arrange -----
        // Create an invalid unit with an empty Name
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidNameUnit") with { Name = "" }; // Explicitly empty Name
        // Create a valid unit to ensure it's still loaded
        var validUnit = CreateDefaultValidUnit(id: "ValidNameUnit", name: "Valid Name");
        var expectedUnits = new List<UnitDefinition> { validUnit }; // Only expect the valid one

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit) // Add the invalid one
            .WithUnit(validUnit)   // Add the valid one
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering()); // Ensure only the valid one is present
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Attack()
    {
        // Arrange -----
        // Create an invalid unit with a negative Attack value
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidAttackUnit") with { Attack = -5 };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidAttackUnit", name: "Valid Attack") with { Attack = 5 };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Armour()
    {
        // Arrange -----
        // Create an invalid unit with a negative Armour value
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidArmourUnit") with { Armour = -5 };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidArmourUnit", name: "Valid Armour") with { Armour = 5 };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Shield()
    {
        // Arrange -----
        // Create an invalid unit with a negative Shield value
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidShieldUnit") with { Shield = -5 };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidShieldUnit", name: "Valid Shield") with { Shield = 5 };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Hangar()
    {
        // Arrange -----
        // Create an invalid unit with a negative Hangar value
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidHangarUnit") with { Hangar = -5 };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidHangarUnit", name: "Valid Hangar") with { Hangar = 5 };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_Speed()
    {
        // Arrange -----
        // Create an invalid unit with a negative Speed value
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSpeedUnit") with { Speed = -5 };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidSpeedUnit", name: "Valid Speed") with { Speed = 5 };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_RequiredShipyard_BaseLevel()
    {
        // Arrange -----
        // Create an invalid unit with a negative BaseLevel in RequiredShipyard
        var invalidRequirement = new ShipyardRequirement(BaseLevel: -1, OrbitalLevel: 0);
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSYBaseUnit") with { RequiredShipyard = invalidRequirement };
        // Create a valid unit
        var validRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: 0);
        var validUnit = CreateDefaultValidUnit(id: "ValidSYBaseUnit", name: "Valid SY Base") with { RequiredShipyard = validRequirement };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Negative_RequiredShipyard_OrbitalLevel()
    {
        // Arrange -----
        // Create an invalid unit with a negative OrbitalLevel in RequiredShipyard
        var invalidRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: -1); // Negative OrbitalLevel
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidSYOrbitalUnit") with { RequiredShipyard = invalidRequirement };
        // Create a valid unit
        var validRequirement = new ShipyardRequirement(BaseLevel: 1, OrbitalLevel: 0); // Valid OrbitalLevel
        var validUnit = CreateDefaultValidUnit(id: "ValidSYOrbitalUnit", name: "Valid SY Orbital") with { RequiredShipyard = validRequirement };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Invalid_RequiredTechnology_TechId()
    {
        // Arrange -----
        // Create an invalid requirement list with an empty TechId
        var invalidReqs = new List<TechRequirement> { new("", 1) }; // Invalid TechId
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidReqTechIdUnit") with { RequiresTechnology = invalidReqs };

        // Create a valid unit
        var validReqs = new List<TechRequirement> { new("ValidTech", 1) };
        var validUnit = CreateDefaultValidUnit(id: "ValidReqTechIdUnit", name: "Valid Req TechId") with { RequiresTechnology = validReqs };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Invalid_RequiredTechnology_Level()
    {
        // Arrange -----
        // Create an invalid requirement list with a Level <= 0
        var invalidReqs = new List<TechRequirement> { new("SomeTech", 0) }; // Invalid Level
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidReqLevelUnit") with { RequiresTechnology = invalidReqs };

        // Create a valid unit
        var validReqs = new List<TechRequirement> { new("SomeTech", 1) }; // Valid Level
        var validUnit = CreateDefaultValidUnit(id: "ValidReqLevelUnit", name: "Valid Req Level") with { RequiresTechnology = validReqs };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Duplicate_RequiredTechnology_TechIds()
    {
        // Arrange -----
        // Create an invalid requirement list with duplicate TechIds
        var invalidReqs = new List<TechRequirement> { new("DupTech", 1), new("DupTech", 2) }; // Duplicate "DupTech"
        var invalidUnit = CreateDefaultValidUnit(id: "DuplicateReqUnit") with { RequiresTechnology = invalidReqs };

        // Create a valid unit (can have multiple, just not duplicates)
        var validReqs = new List<TechRequirement> { new("Tech1", 1), new("Tech2", 1) };
        var validUnit = CreateDefaultValidUnit(id: "NonDuplicateReqUnit", name: "Non-Duplicate Reqs") with { RequiresTechnology = validReqs };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the duplicate check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_DriveType()
    {
        // Arrange -----
        // Create an invalid unit with an empty DriveType
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidDriveUnit") with { DriveType = "" };
        // Create a valid unit
        var validUnit = CreateDefaultValidUnit(id: "ValidDriveUnit", name: "Valid Drive") with { DriveType = "Stellar" };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should fail until we add the check to IsValidUnit
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Missing_WeaponType()
    {
        // Arrange -----
        // Create an invalid unit with an empty WeaponType
        var invalidUnit = CreateDefaultValidUnit(id: "InvalidWeaponUnit") with { DriveType = "Stellar", WeaponType = "" }; // Also give it a valid DriveType

        // Create a valid unit - ENSURE IT HAS A VALID DriveType TOO!
        var validUnit = CreateDefaultValidUnit(id: "ValidWeaponUnit", name: "Valid Weapon") with
        {
            DriveType = "Stellar", // <<< ADD THIS VALID DriveType
            WeaponType = "Laser"
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Use the builder to generate JSON with both units
        var jsonInput = TestHelpers.CreateBuilder()
            .WithUnit(invalidUnit)
            .WithUnit(validUnit)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should now pass because the IsValidUnit check for WeaponType exists
        // and the validUnit now also passes the DriveType check.
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Should_Skip_Unit_With_Null_Entry_In_RequiresTechnology() // Requires manual JSON
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
              "RequiresTechnology": [] 
            }
          ]
        }
        """;

        // Define the expected valid unit - MAKE SURE IT MATCHES THE JSON
        var validUnit = CreateDefaultValidUnit(id: "ValidUnitAlongsideNull", name: "V") with
        {
            DriveType = "Stellar",
            WeaponType = "Laser",
            Attack = 1,
            Armour = 1,
            Speed = 1 // <<< ENSURE Speed MATCHES JSON
                      // Other properties like Shield, Hangar, Cost will use defaults (0, 0, 1) which match JSON
        };
        var expectedUnits = new List<UnitDefinition> { validUnit };

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Units.Should().NotBeNull();
        // This assertion should now PASS
        result.Units.Should().BeEquivalentTo(expectedUnits, options => options.WithStrictOrdering());
    }

    // Future unit tests...
}