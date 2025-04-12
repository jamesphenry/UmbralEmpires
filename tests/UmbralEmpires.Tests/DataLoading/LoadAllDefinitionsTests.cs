// tests/UmbralEmpires.Tests/DataLoading/LoadAllDefinitionsTests.cs
using Xunit;
using FluentAssertions;
using System;
using System.Text.Json;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.Helpers; // Using helpers

namespace UmbralEmpires.Tests.DataLoading;

public class LoadAllDefinitionsTests
{
    private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

    [Fact]
    public void Should_Return_Empty_Lists_For_Empty_Input_Json()
    {
        // Arrange
        var jsonInput = TestHelpers.CreateBuilder().BuildJson(); // Empty builder

        // Act
        var result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Should().NotBeNull();
        result.Structures.Should().NotBeNull().And.BeEmpty();
        result.Technologies.Should().NotBeNull().And.BeEmpty();
        // Assert other lists are empty...
    }

    [Fact]
    public void Should_Throw_Exception_For_Invalid_Json()
    {
        // Arrange
        var invalidJsonInput = """{"Structures": [ { "Id": "Test" // Malformed """;
        Action act = () => _loader.LoadAllDefinitions(invalidJsonInput);

        // Act & Assert
        act.Should().Throw<InvalidOperationException>().WithInnerException<JsonException>();
    }

    [Fact]
    public void Should_Ignore_Extra_Json_Properties() // Needs manual JSON
    {
        // Arrange
        var jsonInput = """
        {
          "Structures": [
            {
              "Id": "ResearchLabs", "Name": "Research Labs", "BaseCreditsCost": 2,
              "PopulationRequirementPerLevel": 1, 
              "ExtraProperty": "Should Be Ignored"
            }
          ],
          "Technologies": []
        }
        """;
        // Expected object only includes properties DEFINED in StructureDefinition
        var expectedStructure = new StructureDefinition
        {
            Id = "ResearchLabs",
            Name = "Research Labs",
            BaseCreditsCost = 2,
            PopulationRequirementPerLevel = 1
            // Other defined properties will have their defaults (0, false, empty list etc.)
        };

        // Act
        BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

        // Assert
        result.Structures.Should().NotBeNull();
        result.Structures.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedStructure, options => options.ExcludingMissingMembers());
        // Using ExcludingMissingMembers might be safer if expectedStructure doesn't explicitly list ALL defaults
    }
}