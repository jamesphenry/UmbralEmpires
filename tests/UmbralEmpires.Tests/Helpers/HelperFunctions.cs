using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Tests.TestDataBuilders;

namespace UmbralEmpires.Tests.Helpers;

public static class TestHelpers
{
    // Generic static helper method to test loading a single definition with specific properties
    public static void TestSingleDefinitionProperty<TDefinition>(
        IDefinitionLoader loader, // Pass loader instance in
        BaseModDefinitionsBuilder builder, // Pass builder instance in
        TDefinition expectedDefinition,
        Func<BaseModDefinitionsBuilder, BaseModDefinitionsBuilder> builderSetup,
        Func<BaseModDefinitions, IEnumerable<TDefinition>?> listSelector)
        where TDefinition : class
    {
        // Arrange
        builder = builderSetup(builder); // Use the provided func to add the item via the builder
        var jsonInput = builder.BuildJson();

        // Act
        BaseModDefinitions result = loader.LoadAllDefinitions(jsonInput);

        // Assert
        var resultList = listSelector(result);
        resultList.Should().NotBeNull();
        resultList.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedDefinition);
    }

    // We could also move the builder creation or other helpers here if desired
    public static BaseModDefinitionsBuilder CreateBuilder() => new BaseModDefinitionsBuilder();

    // Helper to create a default valid technology with required fields populated
    public static TechnologyDefinition CreateDefaultValidTechnology(
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
}