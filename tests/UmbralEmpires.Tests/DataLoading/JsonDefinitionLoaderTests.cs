// tests/UmbralEmpires.Tests/DataLoading/JsonDefinitionLoaderTests.cs
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using UmbralEmpires.Core.Definitions;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Infrastructure.DataLoading;
using UmbralEmpires.Tests.TestDataBuilders; // Assuming builder is here

namespace UmbralEmpires.Tests.DataLoading
{
    public class JsonDefinitionLoaderTests
    {
        private readonly IDefinitionLoader _loader = new JsonDefinitionLoader();

        // Helper to create a default valid structure for tests that only need one valid entry
        private StructureDefinition CreateValidStructure(string id = "ValidStruct", string name = "Valid Name", int cost = 10)
        {
            return new StructureDefinition { Id = id, Name = name, BaseCreditsCost = cost /* Other props will use defaults */};
        }

        [Fact]
        public void LoadAllDefinitions_Should_Load_Single_Simple_Structure()
        {
            // Arrange
            var expectedStructure = CreateValidStructure(id: "UrbanStructures", name: "Urban Structures", cost: 1);

            var jsonInput = new BaseModDefinitionsBuilder()
                .WithStructure(expectedStructure)
                .BuildJson();

            // Act
            BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

            // Assert
            result.Structures.Should().NotBeNull();
            result.Structures.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedStructure);
            result.Technologies.Should().BeEmpty();
        }

        [Fact]
        public void LoadAllDefinitions_Should_Return_Empty_Structures_For_Empty_Structure_List_In_Json()
        {
            // Arrange
            var jsonInput = new BaseModDefinitionsBuilder() // Builder with no structures added
                .BuildJson();

            // Act
            BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

            // Assert
            result.Structures.Should().NotBeNull().And.BeEmpty();
            result.Technologies.Should().BeEmpty();
        }

        [Fact]
        public void LoadAllDefinitions_Should_Load_Multiple_Simple_Structures()
        {
            // Arrange
            var structure1 = CreateValidStructure(id: "S1", name: "Struct 1", cost: 1);
            var structure2 = CreateValidStructure(id: "S2", name: "Struct 2", cost: 2);
            var expectedStructures = new List<StructureDefinition> { structure1, structure2 };

            var jsonInput = new BaseModDefinitionsBuilder()
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
        public void LoadAllDefinitions_Should_Throw_Exception_For_Invalid_Json() // Cannot use builder
        {
            // Arrange
            var invalidJsonInput = """{"Structures": [ { "Id": "Test", "Name": "Test" // Missing closing brace """;
            Action act = () => _loader.LoadAllDefinitions(invalidJsonInput);

            // Act & Assert
            act.Should().Throw<InvalidOperationException>().WithInnerException<JsonException>();
        }

        [Fact]
        public void LoadAllDefinitions_Should_Skip_Object_With_Missing_Id()
        {
            // Arrange
            var invalidStructure = new StructureDefinition { Name = "Missing ID", BaseCreditsCost = 5 }; // Id defaults to ""
            var validStructure = CreateValidStructure(id: "Valid");
            var expectedStructures = new List<StructureDefinition> { validStructure };

            var jsonInput = new BaseModDefinitionsBuilder()
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
        public void LoadAllDefinitions_Should_Skip_Object_With_Negative_Cost()
        {
            // Arrange
            var invalidStructure = CreateValidStructure(id: "NegCost", name: "Neg", cost: -10);
            var validStructure = CreateValidStructure(id: "PosCost", name: "Pos", cost: 10);
            var expectedStructures = new List<StructureDefinition> { validStructure };

            var jsonInput = new BaseModDefinitionsBuilder()
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
            var invalidStructure = new StructureDefinition { Id = "MissingName", BaseCreditsCost = 5 }; // Name defaults to ""
            var validStructure = CreateValidStructure(id: "ValidName", name: "Val", cost: 15);
            var expectedStructures = new List<StructureDefinition> { validStructure };

            var jsonInput = new BaseModDefinitionsBuilder()
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
        public void LoadAllDefinitions_Should_Ignore_Extra_Json_Properties() // Needs manual JSON
        {
            // Arrange
            var jsonInput = """
            {
              "Structures": [
                {
                  "Id": "ResearchLabs", "Name": "Research Labs", "BaseCreditsCost": 2,
                  "ExtraProperty": "Should Be Ignored"
                }
              ], "Technologies": []
            }
            """;
            var expectedStructure = CreateValidStructure(id: "ResearchLabs", name: "Research Labs", cost: 2);

            // Act
            BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

            // Assert
            result.Structures.Should().NotBeNull();
            result.Structures.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedStructure);
        }

        // --- Individual Property Loading Tests ---

        [Fact] public void LoadAllDefinitions_Should_Load_EnergyRequirementPerLevel() => TestProperty(s => s with { EnergyRequirementPerLevel = 4 });
        [Fact] public void LoadAllDefinitions_Should_Load_PopulationRequirementPerLevel() => TestProperty(s => s with { PopulationRequirementPerLevel = 1 });
        [Fact] public void LoadAllDefinitions_Should_Load_AreaRequirementPerLevel() => TestProperty(s => s with { AreaRequirementPerLevel = 1 });
        [Fact] public void LoadAllDefinitions_Should_Load_EconomyBonus() => TestProperty(s => s with { EconomyBonus = 5 });
        [Fact] public void LoadAllDefinitions_Should_Load_IsAdvanced_Flag() => TestProperty(s => s with { IsAdvanced = true });
        [Fact] public void LoadAllDefinitions_Should_Load_BaseConstructionBonus() => TestProperty(s => s with { BaseConstructionBonus = 2 });
        [Fact] public void LoadAllDefinitions_Should_Load_BaseProductionBonus() => TestProperty(s => s with { BaseProductionBonus = 3 });
        [Fact] public void LoadAllDefinitions_Should_Load_BaseResearchBonus() => TestProperty(s => s with { BaseResearchBonus = 8 });
        [Fact] public void LoadAllDefinitions_Should_Load_UsesMetal_Flag() => TestProperty(s => s with { UsesMetal = true });
        [Fact] public void LoadAllDefinitions_Should_Load_UsesGas_Flag() => TestProperty(s => s with { UsesGas = true });
        [Fact] public void LoadAllDefinitions_Should_Load_UsesCrystal_Flag() => TestProperty(s => s with { UsesCrystal = true });
        [Fact] public void LoadAllDefinitions_Should_Load_UsesSolar_Flag() => TestProperty(s => s with { UsesSolar = true });
        [Fact] public void LoadAllDefinitions_Should_Load_AddsPopCapacityByFertility_Flag() => TestProperty(s => s with { AddsPopCapacityByFertility = true });
        [Fact] public void LoadAllDefinitions_Should_Load_AreaCapacityBonus() => TestProperty(s => s with { AreaCapacityBonus = 10 });
        [Fact] public void LoadAllDefinitions_Should_Load_IncreasesAstroFertility_Flag() => TestProperty(s => s with { IncreasesAstroFertility = true });

        [Fact]
        public void LoadAllDefinitions_Should_Load_RequiresTechnology_List()
        {
            // Arrange
            var expectedReq = new List<TechRequirement> { new("TestTech", 1) };
            var expectedStructure = CreateValidStructure(id: "TechReqTest") with { RequiresTechnology = expectedReq };

            var jsonInput = new BaseModDefinitionsBuilder()
                .WithStructure(expectedStructure)
                .BuildJson();

            // Act
            BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

            // Assert
            result.Structures.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedStructure);
        }

        // --- Private Helper for Simple Property Tests ---
        // Reduces boilerplate for tests just checking one property loads
        private void TestProperty(Func<StructureDefinition, StructureDefinition> arrangeFunc)
        {
            // Arrange
            var baseStructure = CreateValidStructure(id: "PropTest"); // Base valid object
            var expectedStructure = arrangeFunc(baseStructure); // Apply the change for the specific property

            var jsonInput = new BaseModDefinitionsBuilder()
                .WithStructure(expectedStructure) // Build JSON based on expected state
                .BuildJson();

            // Act
            BaseModDefinitions result = _loader.LoadAllDefinitions(jsonInput);

            // Assert
            result.Structures.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedStructure);
        }
    }
}