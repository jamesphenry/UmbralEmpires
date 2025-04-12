﻿using Xunit;
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

    // Future unit tests...
}