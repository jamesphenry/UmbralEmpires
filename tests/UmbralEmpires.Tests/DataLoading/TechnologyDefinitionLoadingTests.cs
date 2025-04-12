﻿// tests/UmbralEmpires.Tests/DataLoading/TechnologyDefinitionLoadingTests.cs (New File)
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.TestDataBuilders;
using UmbralEmpires.Tests.Helpers;

namespace UmbralEmpires.Tests.DataLoading;

public class TechnologyDefinitionLoadingTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // Helper to create a default valid technology
    private TechnologyDefinition CreateDefaultValidTechnology(string id = "ValidTech", string name = "Valid Tech Name", int cost = 1, int labs = 1)
    {
        return new TechnologyDefinition
        {
            Id = id,
            Name = name,
            CreditsCost = cost,
            RequiredLabsLevel = labs,
            RequiresPrerequisites = new List<TechRequirement>() // Default empty list
            // Description defaults to ""
        };
    }

    [Fact]
    public void Should_Load_Single_Simple_Technology()
    {
        // Arrange
        var expected = CreateDefaultValidTechnology(id: "Energy", name: "Energy", cost: 2, labs: 1) with { Description = "Increases all bases energy output by 5%." };

        // Act & Assert using helper
        TestHelpers.TestSingleDefinitionProperty(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected), // Use WithTechnology
            r => r.Technologies // Select Technologies list
        );
    }

    [Fact]
    public void Should_Load_Technology_With_Prerequisites()
    {
        // Arrange
        var expectedReqs = new List<TechRequirement> { new("Energy", 2) };
        var expected = CreateDefaultValidTechnology(id: "Laser", name: "Laser", cost: 4, labs: 2) with
        {
            RequiresPrerequisites = expectedReqs,
            Description = "Increases laser weapons power by 5%."
        };

        // Act & Assert using helper
        TestHelpers.TestSingleDefinitionProperty(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected),
            r => r.Technologies
        );
    }

    [Fact]
    public void LoadAllDefinitions_Should_Load_Single_Simple_Technology()
    {
        // Arrange
        var expected = CreateDefaultValidTechnology(id: "Energy", name: "Energy", cost: 2, labs: 1) with { Description = "Increases all bases energy output by 5%." };

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<TechnologyDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected), // Use WithTechnology
            r => r.Technologies             // Select Technologies list
        );

        // Additionally verify Structures list is empty
        var builder = TestHelpers.CreateBuilder().WithTechnology(expected);
        var jsonInput = builder.BuildJson();
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        result.Structures.Should().BeEmpty();
    }

    [Fact]
    public void Should_Skip_Technology_With_Negative_Cost()
    {
        // Arrange -----
        var invalidTech = CreateDefaultValidTechnology(id: "NegCostTech", cost: -100);
        var validTech = CreateDefaultValidTechnology(id: "PosCostTech", cost: 100);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech }; // Only expect the valid one

        // Use the builder to generate JSON with both techs
        var jsonInput = TestHelpers.CreateBuilder()
            .WithTechnology(invalidTech)
            .WithTechnology(validTech)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Technologies.Should().NotBeNull();
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies); // Implicitly checks count and content

        // --- TEMPORARY Assert if needed ---
        // Assert.True(false, "Verify implementation skips techs with negative cost.");
    }

    [Fact]
    public void Should_Skip_Technology_With_Negative_LabsLevel()
    {
        // Arrange -----
        var invalidTech = CreateDefaultValidTechnology(id: "NegLabsTech", labs: -1); // Set invalid labs level
        var validTech = CreateDefaultValidTechnology(id: "PosLabsTech", labs: 1);  // Valid labs level
        var expectedTechnologies = new List<TechnologyDefinition> { validTech }; // Only expect the valid one

        // Use the builder to generate JSON with both techs
        var jsonInput = TestHelpers.CreateBuilder()
            .WithTechnology(invalidTech)
            .WithTechnology(validTech)
            .BuildJson();

        // Act -----
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert -----
        result.Technologies.Should().NotBeNull();
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies); // Implicitly checks count and content
    }

    [Fact]
    public void Should_Load_Technology_Description()
    {
        // Arrange -----
        var expectedDescription = "Test description for loading.";
        // Use the helper to create a base valid tech, then use 'with' to set the description
        var expected = CreateDefaultValidTechnology(id: "DescTest") with { Description = expectedDescription };

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<TechnologyDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected), // Builder adds the tech with the description
            r => r.Technologies             // Select Technologies list
        );

        // --- TEMPORARY Assert if needed ---
        // Assert.True(false, "Verify Description field is loaded.");
    }

    // --- Add more tests for technologies ---
    // - Loading multiple techs
    // - Skipping techs with missing Id/Name
    // - Skipping techs with negative cost/labs level
    // - Loading all properties
    // etc...

}