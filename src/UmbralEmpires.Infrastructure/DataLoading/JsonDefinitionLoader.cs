// src/UmbralEmpires.Infrastructure/DataLoading/JsonDefinitionLoader.cs
using System; // For ArgumentNullException
using System.Collections.Generic;
using System.Text.Json;
using UmbralEmpires.Application.Interfaces; // Or namespace where IDefinitionLoader lives
using UmbralEmpires.Core.Definitions;    // Or namespace where StructureDefinition lives

namespace UmbralEmpires.Infrastructure.DataLoading;

public class JsonDefinitionLoader : IDefinitionLoader
{
    public IEnumerable<StructureDefinition> LoadStructures(string jsonContent)
    {
        // Minimal implementation to make the first test pass
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            // Return empty list for null/empty input (simplest passing behavior)
            return new List<StructureDefinition>();
            // Could throw ArgumentNullException - depends on desired contract, add test later
        }

        try
        {
            // Basic deserialization using System.Text.Json
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Allow "id" or "Id" in JSON
            };
            var definitions = JsonSerializer.Deserialize<List<StructureDefinition>>(jsonContent, options);

            // Return the deserialized list, or an empty list if null
            return definitions ?? new List<StructureDefinition>();
        }
        catch (JsonException ex)
        {
            // Basic error handling for invalid JSON - just return empty for now to pass test expecting valid input
            Console.WriteLine($"Error deserializing structure definitions: {ex.Message}"); // Logging TBD
            return new List<StructureDefinition>(); // Or throw? Add test later.
        }
    }
}