// tests/UmbralEmpires.Tests/DataLoading/TechnologyDefinitionLoadingTests.cs
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

public class TechnologyDefinitionLoadingTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    // ---> HELPER METHOD DEFINITION WAS MISSING - ADD THIS <---
    // Helper to create a default valid technology with required fields populated
    private TechnologyDefinition CreateDefaultValidTechnology(
        string id = "ValidTech",
        string name = "Valid Tech Name",
        int cost = 1,
        int labs = 1)
    {
        // Ensure all properties expected by BeEquivalentTo have reasonable defaults
        return new TechnologyDefinition
        {
            Id = id,
            Name = name,
            CreditsCost = cost,
            RequiredLabsLevel = labs,
            RequiresPrerequisites = new List<TechRequirement>(), // Default empty list
            Description = "" // Default empty string
        };
    }
    // ---> END HELPER METHOD DEFINITION <---

    [Fact]
    public void Should_Load_Single_Simple_Technology()
    {
        // Arrange
        var expected = CreateDefaultValidTechnology(id: "Energy", name: "Energy", cost: 2, labs: 1) with { Description = "Increases all bases energy output by 5%." };

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<TechnologyDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected),
            r => r.Technologies
        );

        // Additionally verify other lists are empty
        var builder = TestHelpers.CreateBuilder().WithTechnology(expected);
        var jsonInput = builder.BuildJson();
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        result.Structures.Should().BeEmpty();
        result.Units.Should().BeEmpty(); // Assuming Units list exists in BaseModDefinitions now
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

        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<TechnologyDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected),
            r => r.Technologies
        );
    }

    [Fact]
    public void Should_Skip_Technology_With_Negative_Cost()
    {
        // Arrange
        var invalidTech = CreateDefaultValidTechnology(id: "NegCostTech", cost: -100);
        var validTech = CreateDefaultValidTechnology(id: "PosCostTech", cost: 100);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };
        var jsonInput = TestHelpers.CreateBuilder().WithTechnology(invalidTech).WithTechnology(validTech).BuildJson();
        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    [Fact]
    public void Should_Skip_Technology_With_Negative_LabsLevel()
    {
        // Arrange
        var invalidTech = CreateDefaultValidTechnology(id: "NegLabsTech", labs: -1);
        var validTech = CreateDefaultValidTechnology(id: "PosLabsTech", labs: 1);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };
        var jsonInput = TestHelpers.CreateBuilder().WithTechnology(invalidTech).WithTechnology(validTech).BuildJson();
        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    [Fact]
    public void Should_Load_Technology_Description()
    {
        // Arrange
        var expectedDescription = "This is the test description.";
        var expected = CreateDefaultValidTechnology(id: "DescTest") with { Description = expectedDescription };
        // Act & Assert using static helper
        TestHelpers.TestSingleDefinitionProperty<TechnologyDefinition>(
            _loader, TestHelpers.CreateBuilder(), expected,
            b => b.WithTechnology(expected),
            r => r.Technologies
        );
    }

    [Fact]
    public void Should_Skip_Technology_With_Invalid_Prerequisite_TechId()
    {
        // Arrange
        var invalidPrereqs = new List<TechRequirement> { new("", 5) }; // Invalid TechId
        var invalidTech = CreateDefaultValidTechnology(id: "InvalidReqTechId") with { RequiresPrerequisites = invalidPrereqs };
        var validTech = CreateDefaultValidTechnology(id: "ValidReqTechId", cost: 50);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };
        var jsonInput = TestHelpers.CreateBuilder().WithTechnology(invalidTech).WithTechnology(validTech).BuildJson();
        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    [Fact]
    public void Should_Skip_Technology_With_Invalid_Prerequisite_Level()
    {
        // Arrange
        var invalidPrereqs = new List<TechRequirement> { new("Energy", 0) }; // Invalid Level
        var invalidTech = CreateDefaultValidTechnology(id: "InvalidReqLvlTech") with { RequiresPrerequisites = invalidPrereqs };
        var validTech = CreateDefaultValidTechnology(id: "ValidReqLvlTech", cost: 50);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };
        var jsonInput = TestHelpers.CreateBuilder().WithTechnology(invalidTech).WithTechnology(validTech).BuildJson();
        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    [Fact]
    public void Should_Skip_Technology_With_Null_Entry_In_Prerequisites_List() // Requires manual JSON
    {
        // Arrange
        var jsonInput = """
        {
          "Structures": [],
          "Technologies": [
            { "Id": "TechWithNullReq", "Name": "T", "CreditsCost": 10, "RequiredLabsLevel": 1, "RequiresPrerequisites": [ null ] },
            { "Id": "ValidTechAlongsideNull", "Name": "V", "CreditsCost": 20, "RequiredLabsLevel": 1, "RequiresPrerequisites": [] }
          ]
        }
        """;
        var validTech = CreateDefaultValidTechnology(id: "ValidTechAlongsideNull", name: "V", cost: 20, labs: 1); // Match valid tech
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    [Fact]
    public void Should_Skip_Technology_With_Duplicate_Prerequisite_TechIds()
    {
        // Arrange
        var invalidPrereqs = new List<TechRequirement> { new("Energy", 1), new("Energy", 2) }; // Duplicate Energy
        var invalidTech = CreateDefaultValidTechnology(id: "DupReqTech") with { RequiresPrerequisites = invalidPrereqs };
        var validTech = CreateDefaultValidTechnology(id: "ValidTechAgain", cost: 50);
        var expectedTechnologies = new List<TechnologyDefinition> { validTech };
        var jsonInput = TestHelpers.CreateBuilder().WithTechnology(invalidTech).WithTechnology(validTech).BuildJson();
        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);
        // Assert
        result.Technologies.Should().BeEquivalentTo(expectedTechnologies);
    }

    // Future technology tests... Maybe validation that Prereq TechID exists?
}