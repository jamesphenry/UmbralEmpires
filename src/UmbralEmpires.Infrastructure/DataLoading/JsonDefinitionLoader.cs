// src/UmbralEmpires.Infrastructure/DataLoading/JsonDefinitionLoader.cs
using System;
using System.Collections.Generic;
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
            // Existing handling for empty/null input
            return new List<StructureDefinition>();
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var definitions = JsonSerializer.Deserialize<List<StructureDefinition>>(jsonContent, options);
            return definitions ?? new List<StructureDefinition>();
        }
        catch (JsonException ex)
        {
            // --- MODIFIED SECTION ---
            // Instead of just logging and returning empty, re-throw the exception
            // This makes the test expecting a JsonException pass.
            Console.WriteLine($"Error deserializing structure definitions: {ex.Message}"); // Keep logging for now
            throw; // Re-throw the original JsonException
            // --- END MODIFIED SECTION ---

            // Alternative for later refactor: wrap in custom exception
            // throw new DefinitionLoadException("Failed to parse structure definitions due to invalid JSON.", ex);
        }
    }

    // Placeholder for a potential custom exception (defined elsewhere, maybe Core or Application)
    // public class DefinitionLoadException : Exception { /* ... constructors ... */ }
}