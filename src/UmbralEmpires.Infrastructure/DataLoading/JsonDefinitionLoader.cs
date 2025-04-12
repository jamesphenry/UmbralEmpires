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
    // --- NEW METHOD ---
    public BaseModDefinitions LoadAllDefinitions(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            Console.WriteLine("Warning: Definition content is empty, returning empty definitions.");
            return new BaseModDefinitions(); // Return empty container
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            // Deserialize the entire structure
            var loadedData = JsonSerializer.Deserialize<BaseModDefinitions>(jsonContent, options);

            if (loadedData == null)
            {
                Console.WriteLine("Warning: Deserialization resulted in null BaseModDefinitions object.");
                return new BaseModDefinitions(); // Return empty container
            }

            // --- Apply Validation/Filtering ---
            // Filter Structures (using existing helper method)
            var initialStructureCount = loadedData.Structures?.Count ?? 0;
            var validStructures = loadedData.Structures?
                                      .Where(IsValidStructure) // Apply validation
                                      .ToList() ?? new List<StructureDefinition>();
            if (validStructures.Count < initialStructureCount)
                Console.WriteLine($"Warning: Skipped {initialStructureCount - validStructures.Count} structure(s) due to validation errors.");

            // Filter Technologies (using new helper method - create below)
            var initialTechCount = loadedData.Technologies?.Count ?? 0;
            var validTechnologies = loadedData.Technologies?
                                      .Where(IsValidTechnology) // Apply validation
                                      .ToList() ?? new List<TechnologyDefinition>();
            if (validTechnologies.Count < initialTechCount)
                Console.WriteLine($"Warning: Skipped {initialTechCount - validTechnologies.Count} technology(s) due to validation errors.");

            // Filter Units... (when UnitDefinition exists)
            // Filter Defenses... (when DefenseDefinition exists)


            // Return a NEW object containing only the validated lists
            return loadedData with // Using record "with" expression
            {
                Structures = validStructures,
                Technologies = validTechnologies
                // Assign validated lists for Units, Defenses etc. here later
            };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing base definitions: {ex.Message}");
            // Re-throw to indicate catastrophic failure loading definitions
            throw new InvalidOperationException("Failed to load base game definitions due to invalid JSON.", ex);
        }
    }

    // Existing helper for structures
    private bool IsValidStructure(StructureDefinition? structure)
    {
        if (structure == null) return false;
        if (string.IsNullOrWhiteSpace(structure.Id)) return false;
        if (structure.BaseCreditsCost < 0) return false;
        if (string.IsNullOrWhiteSpace(structure.Name)) return false;
        // Add more checks here...
        return true;
    }

    // --- NEW HELPER METHOD (Placeholder) ---
    private bool IsValidTechnology(TechnologyDefinition? tech)
    {
        if (tech == null) return false;
        if (string.IsNullOrWhiteSpace(tech.Id)) return false;
        if (string.IsNullOrWhiteSpace(tech.Name)) return false;
        if (tech.CreditsCost < 0) return false; // Costs should be non-negative
        if (tech.RequiredLabsLevel < 0) return false; // Level should be non-negative
                                                      // Add more checks (e.g., validate prerequisite IDs exist?) later
        return true;
    }

    // Implement IsValidUnit, IsValidDefense later..
    public IEnumerable<StructureDefinition> LoadStructures(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return Enumerable.Empty<StructureDefinition>(); // Prefer Enumerable.Empty for empty lists
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var initialList = JsonSerializer.Deserialize<List<StructureDefinition>>(jsonContent, options);

            if (initialList == null)
            {
                Console.WriteLine("DEBUG: Deserialization resulted in null initialList.");
                return Enumerable.Empty<StructureDefinition>();
            }

            var validList = initialList
                        .Where(IsValidStructure) // Use the helper method
                        .ToList();

            // Optional logging...
            if (validList.Count < initialList.Count) { /* ... logging ... */ }


            return validList;
        }
        catch (JsonException ex)
        {
            // ... exception handling ...
            throw;
        }
    }
}