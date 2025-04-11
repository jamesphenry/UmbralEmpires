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

            // ---> Add Enhanced Debugging <---
            Console.WriteLine($"DEBUG: Initial count: {initialList.Count}");
            // Use a loop for safer checking in case deserialization created null entries (unlikely but possible)
            for (int i = 0; i < initialList.Count; i++)
            {
                var item = initialList[i];
                var idValue = item?.Id ?? "NULL_ITEM_ID"; // Handle null item just in case
                var isIdNullOrWhitespace = string.IsNullOrWhiteSpace(idValue);
                Console.WriteLine($"DEBUG: Initial item {i} -> Id: '{idValue}', IsNullOrWhiteSpace(Id): {isIdNullOrWhitespace}");
            }
            // ---> End Enhanced Debugging <---

            var validList = initialList
                .Where(structure => structure != null && !string.IsNullOrWhiteSpace(structure.Id)) // Added null check on structure for safety
                .ToList();

            // Optional logging...
            if (validList.Count < initialList.Count) { /* ... logging ... */ }

            // ---> Add Debug Line Before Return <---
            Console.WriteLine($"DEBUG: Filtered count: {validList.Count}");
            for (int i = 0; i < validList.Count; i++)
            {
                Console.WriteLine($"DEBUG: Filtered item {i} -> Id: '{validList[i]?.Id ?? "NULL_ITEM_ID"}'");
            }
            // ---> End Debug Line Before Return <---

            return validList;
        }
        catch (JsonException ex)
        {
            // ... exception handling ...
            throw;
        }
    }
}