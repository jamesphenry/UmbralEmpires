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

    private bool IsValidStructure(StructureDefinition? structure)
    {
        if (structure == null) return false;
        if (string.IsNullOrWhiteSpace(structure.Id)) return false;
        // Add more checks here later...
        // if (string.IsNullOrWhiteSpace(structure.Name)) return false;
        // if (structure.BaseCreditsCost < 0) return false;
        return true;
    }


}