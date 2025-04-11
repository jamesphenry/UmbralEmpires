// src/UmbralEmpires.Infrastructure/DataLoading/JsonDefinitionLoader.cs
using System;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ Where clause
using System.Text.Json;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Core.Definitions;

namespace UmbralEmpires.Infrastructure.DataLoading;

public class JsonDefinitionLoader : IDefinitionLoader
{
    public IEnumerable<StructureDefinition> LoadStructures(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return Enumerable.Empty<StructureDefinition>(); // Prefer Enumerable.Empty for empty lists
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            // Step 1: Deserialize the whole list (might contain invalid items)
            var initialList = JsonSerializer.Deserialize<List<StructureDefinition>>(jsonContent, options);

            if (initialList == null)
            {
                return Enumerable.Empty<StructureDefinition>();
            }

            // --- NEW SECTION: Validation/Filtering ---
            // Step 2: Filter the list to include only valid definitions
            // Basic validation: Ensure required 'Id' property is present.
            // This implements the core idea of the IsValid() check from pseudocode for this specific test.
            var validList = initialList
                .Where(structure => !string.IsNullOrWhiteSpace(structure.Id))
                .ToList(); // Convert back to List or keep as IEnumerable

            // Log warnings for skipped items? Could be added here or in validation logic.
            if (validList.Count < initialList.Count)
            {
                Console.WriteLine($"Warning: Skipped {initialList.Count - validList.Count} structure(s) due to missing required properties (e.g., Id)."); // Basic logging
            }
            // --- END NEW SECTION ---

            // Step 3: Return the filtered list
            return validList;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing structure definitions: {ex.Message}");
            throw; // Re-throw as per previous test
        }
    }
}