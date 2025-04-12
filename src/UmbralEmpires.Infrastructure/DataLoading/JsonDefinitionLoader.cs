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

            // ---> ADD Unit Validation <---
            var initialUnitCount = loadedData.Units?.Count ?? 0;
            var validUnits = loadedData.Units?
                                      .Where(IsValidUnit) // Apply validation
                                      .ToList() ?? new List<UnitDefinition>();
            if (validUnits.Count < initialUnitCount)
                Console.WriteLine($"Warning: Skipped {initialUnitCount - validUnits.Count} unit(s) due to validation errors.");
            // ---> END Unit Validation <---


            // Filter Units... (when UnitDefinition exists)
            // Filter Defenses... (when DefenseDefinition exists)


            // Return a NEW object containing only the validated lists
            return loadedData with // Using record "with" expression
            {
                Structures = validStructures,
                Technologies = validTechnologies,
                Units = validUnits
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

    private bool IsValidUnit(UnitDefinition? unit)
    {
        if (unit == null) return false;
        if (string.IsNullOrWhiteSpace(unit.Id)) return false;
        if (string.IsNullOrWhiteSpace(unit.Name)) return false;
        if (unit.CreditsCost < 0) return false;
        if (unit.Attack < 0) return false;
        if (unit.Armour < 0) return false;
        if (unit.Shield < 0) return false;
        if (unit.Hangar < 0) return false;
        if (unit.Speed < 0) return false;

        // Check the ShipyardRequirement
        if (unit.RequiredShipyard == null) return false;
        if (unit.RequiredShipyard.BaseLevel < 0) return false;
        if (unit.RequiredShipyard.OrbitalLevel < 0) return false;

        // Check RequiresTechnology list
        if (unit.RequiresTechnology != null)
        {
            foreach (var requirement in unit.RequiresTechnology)
            {
                if (requirement == null) return false;
                if (string.IsNullOrWhiteSpace(requirement.TechId)) return false;
                if (requirement.Level <= 0) return false;
            }

            var hasDuplicates = unit.RequiresTechnology
                                    .GroupBy(r => r.TechId)
                                    .Any(g => g.Count() > 1);
            if (hasDuplicates)
            {
                return false;
            }
        }

        // ---> MODIFY DriveType Check <---
        // Define valid drive types
        var validDriveTypes = new HashSet<string> { "Interceptor", "Stellar", "Warp" };
        if (string.IsNullOrWhiteSpace(unit.DriveType) || !validDriveTypes.Contains(unit.DriveType))
        {
            // Optional: Log warning about unknown drive type
            // Console.WriteLine($"Warning: Skipping unit ID '{unit.Id}' due to unknown DriveType: {unit.DriveType}");
            return false;
        }
        // ---> END MODIFIED CHECK <---

        if (string.IsNullOrWhiteSpace(unit.WeaponType)) return false;

        // Add more checks later based on tests...
        return true;
    }

    // Current IsValidTechnology method
    private bool IsValidTechnology(TechnologyDefinition? tech)
    {
        if (tech == null) return false;
        if (string.IsNullOrWhiteSpace(tech.Id)) return false;
        if (string.IsNullOrWhiteSpace(tech.Name)) return false;
        if (tech.CreditsCost < 0) return false;
        if (tech.RequiredLabsLevel < 0) return false;

        if (tech.RequiresPrerequisites != null)
        {
            // Check individual prerequisites first
            foreach (var requirement in tech.RequiresPrerequisites)
            {
                if (requirement == null) return false;
                if (string.IsNullOrWhiteSpace(requirement.TechId)) return false;
                if (requirement.Level <= 0) return false;
            }

            // ---> ADD THIS CHECK for Duplicate Prerequisite TechIDs <---
            // Check if there are any TechIds that appear more than once in the list
            var hasDuplicates = tech.RequiresPrerequisites
                                    .GroupBy(r => r.TechId) // Group by TechId
                                    .Any(g => g.Count() > 1); // Check if any group has more than one item

            if (hasDuplicates)
            {
                Console.WriteLine($"Warning: Skipping tech ID '{tech.Id}' due to duplicate prerequisite TechIds."); // Optional warning
                return false; // Found duplicates
            }
            // ---> END ADDED CHECK <---
        }

        return true; // Passes all checks
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