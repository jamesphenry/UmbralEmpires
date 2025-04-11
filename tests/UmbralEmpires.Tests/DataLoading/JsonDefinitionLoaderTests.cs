// Add necessary using statements at the top of the file
using FluentAssertions;
using UmbralEmpires.Core.Definitions;       // Assuming this namespace for StructureDefinition
using UmbralEmpires.Application.Interfaces; // Assuming this namespace for IDefinitionLoader
using UmbralEmpires.Infrastructure.DataLoading; // Assuming this namespace for JsonDefinitionLoader

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

        // Future tests will go here...
    }
}