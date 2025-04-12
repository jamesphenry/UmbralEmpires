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

    // --- Add more tests for technologies ---
    // - Loading multiple techs
    // - Skipping techs with missing Id/Name
    // - Skipping techs with negative cost/labs level
    // - Loading all properties
    // etc...

}