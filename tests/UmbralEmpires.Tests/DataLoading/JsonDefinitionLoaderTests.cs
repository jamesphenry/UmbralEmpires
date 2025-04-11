// Add necessary using statements at the top of the file
using FluentAssertions;
using UmbralEmpires.Core.Definitions;       // Assuming this namespace for StructureDefinition
using UmbralEmpires.Application.Interfaces; // Assuming this namespace for IDefinitionLoader
using UmbralEmpires.Infrastructure.DataLoading;
using System.Text.Json; // Assuming this namespace for JsonDefinitionLoader

namespace UmbralEmpires.Tests.DataLoading
{
    public class JsonDefinitionLoaderTests
    {
        [Fact]
        public void LoadStructures_Should_Load_Single_Simple_Structure_From_Json()
        {
            // Arrange -----

            // Input JSON string (matches the minimal StructureDefinition properties)
            var jsonInput = """
            [
              {
                "Id": "UrbanStructures",
                "Name": "Urban Structures",
                "BaseCreditsCost": 1
              }
            ]
            """;

            // Expected output object
            var expectedStructure = new StructureDefinition
            {
                Id = "UrbanStructures",
                Name = "Urban Structures",
                BaseCreditsCost = 1
            };

            // Instantiate the concrete loader implementation
            IDefinitionLoader loader = new JsonDefinitionLoader();


            // Act -----

            // Call the method under test
            IEnumerable<StructureDefinition> result = loader.LoadStructures(jsonInput);


            // Assert -----

            // Use FluentAssertions to check the result
            result.Should().NotBeNull(); // Ensure the result isn't null
            result.Should().ContainSingle() // Ensure there's exactly one item in the collection
                  .Which.Should().BeEquivalentTo(expectedStructure); // Ensure the item matches the expected data
        }

        [Fact]
        public void LoadStructures_Should_Return_Empty_List_For_Empty_Json_Array()
        {
            // Arrange -----
            var jsonInput = "[]"; // Empty JSON array
            IDefinitionLoader loader = new JsonDefinitionLoader();

            // Act -----
            IEnumerable<StructureDefinition> result = loader.LoadStructures(jsonInput);

            // Assert -----
            result.Should().NotBeNull(); // Should return an empty list, not null
            result.Should().BeEmpty(); // Should contain no elements

            // --- TEMPORARY Assert if needed until code verified ---
            // Assert.True(false, "Verify implementation handles empty array correctly.");
        }

        [Fact]
        public void LoadStructures_Should_Load_Multiple_Simple_Structures_From_Json()
        {
            // Arrange -----
            var jsonInput = """
            [
              {
                "Id": "UrbanStructures",
                "Name": "Urban Structures",
                "BaseCreditsCost": 1
              },
              {
                "Id": "ResearchLabs",
                "Name": "Research Labs",
                "BaseCreditsCost": 2
              }
            ]
            """;

            var expectedStructures = new List<StructureDefinition>
            {
                new StructureDefinition { Id = "UrbanStructures", Name = "Urban Structures", BaseCreditsCost = 1 },
                new StructureDefinition { Id = "ResearchLabs", Name = "Research Labs", BaseCreditsCost = 2 }
            };

            IDefinitionLoader loader = new JsonDefinitionLoader();

            // Act -----
            IEnumerable<StructureDefinition> result = loader.LoadStructures(jsonInput);

            // Assert -----
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Verify we loaded two items
            // BeEquivalentTo checks if the collections contain the same items, ignoring order by default
            result.Should().BeEquivalentTo(expectedStructures);

            // --- TEMPORARY Assert if needed ---
            // Assert.True(false, "Verify implementation handles multiple items correctly.");
        }

        [Fact]
        public void LoadStructures_Should_Throw_Exception_For_Invalid_Json()
        {
            // Arrange -----
            var invalidJsonInput = """
            [
              {
                "Id": "UrbanStructures",
                "Name": "Urban Structures", // Missing comma here
                "BaseCreditsCost": 1
              }
            ]
            """; // Malformed JSON

            IDefinitionLoader loader = new JsonDefinitionLoader();

            // Act -----
            // Use an Action delegate to wrap the call that should throw
            Action act = () => loader.LoadStructures(invalidJsonInput);

            // Assert -----
            // Assert that the action throws the expected exception
            // System.Text.Json typically throws JsonException for parsing errors
            // removed .WithMessage("*invalid JSON*") to avoid hardcoding error messages
            act.Should().Throw<JsonException>();

            // --- TEMPORARY Assert if needed ---
            // Assert.True(false, "Verify implementation throws exception on invalid JSON.");
        }

        [Fact]
        public void LoadStructures_Should_Skip_Object_With_Missing_Required_Property_And_Load_Valid_Ones()
        {
            // Arrange -----
            var jsonInput = """
            [
              {
                "Name": "Structure Missing ID", 
                "BaseCreditsCost": 5
              },
              {
                "Id": "ResearchLabs",
                "Name": "Research Labs",
                "BaseCreditsCost": 2 
              }
            ]
            """;

            // We only expect the valid structure to be loaded
            var expectedValidStructure = new StructureDefinition
            {
                Id = "ResearchLabs",
                Name = "Research Labs",
                BaseCreditsCost = 2
            };

            IDefinitionLoader loader = new JsonDefinitionLoader();

            // Act -----
            IEnumerable<StructureDefinition> result = loader.LoadStructures(jsonInput);

            // Assert -----
            result.Should().NotBeNull();
            // Should only contain the valid structure, the one missing 'Id' should be skipped
            result.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedValidStructure);

            // Ideally, we might also assert that a warning was logged, but testing logging can be complex.
            // For now, just ensuring the invalid one is skipped is the primary goal.

            // --- TEMPORARY Assert if needed ---
            // Assert.True(false, "Verify implementation skips objects with missing required properties.");
        }
    }
}