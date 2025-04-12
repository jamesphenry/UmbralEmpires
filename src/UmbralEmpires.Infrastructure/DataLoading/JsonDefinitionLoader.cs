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

        // Check RequiresTechnology list structure first
        bool requiresTechnologyListValid = true;
        List<TechRequirement>? techReqs = unit.RequiresTechnology; // Cache for slightly cleaner access

        if (techReqs != null)
        {
            foreach (var requirement in techReqs)
            {
                // Check for null item, invalid TechId, or invalid Level
                if (requirement == null || string.IsNullOrWhiteSpace(requirement.TechId) || requirement.Level <= 0)
                {
                    requiresTechnologyListValid = false;
                    break;
                }
            }

            // Only check for duplicates if individual items are structurally valid
            if (requiresTechnologyListValid)
            {
                var hasDuplicates = techReqs
                                    .GroupBy(r => r.TechId, StringComparer.OrdinalIgnoreCase)
                                    .Any(g => g.Count() > 1);
                if (hasDuplicates)
                {
                    requiresTechnologyListValid = false;
                }
            }
        }
        // If techReqs list itself was null, requiresTechnologyListValid remains true (list structure is ok)

        // Fail early if the list structure was bad (null items, bad id/level, duplicates)
        if (!requiresTechnologyListValid) return false;

        // --- Drive Type and Related Tech Validation ---
        // Define valid drive types (Corrected: using "Inter")
        var validDriveTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Inter", "Stellar", "Warp" };

        string? driveType = unit.DriveType; // Cache for slightly cleaner access

        // Check if drive type string itself is valid
        if (string.IsNullOrWhiteSpace(driveType) || !validDriveTypes.Contains(driveType))
        {
            return false;
        }

        // Check specific tech requirements based on drive type
        if (driveType.Equals("Stellar", StringComparison.OrdinalIgnoreCase))
        {
            // Must require "Stellar Drive" tech
            bool requiresStellar = techReqs?.Any(req =>
                req.TechId.Equals("Stellar Drive", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!requiresStellar)
            {
                // Console.WriteLine($"Warning: Unit '{unit.Id}' has Stellar drive but is missing 'Stellar Drive' tech requirement.");
                return false; // <<< This is the crucial check for the failing test
            }
        }
        else if (driveType.Equals("Warp", StringComparison.OrdinalIgnoreCase))
        {
            // Placeholder for Warp Drive check - we will add this next
            // bool requiresWarp = techReqs?.Any(req => ...) ?? false;
            // if (!requiresWarp) return false; 
        }
        // No specific *drive* tech needed for "Inter" drive type based on current understanding

        // --- End Drive Type Checks ---

        // Check Weapon Type (IsNullOrWhiteSpace check seems sufficient for now)
        if (string.IsNullOrWhiteSpace(unit.WeaponType)) return false;

        // If all checks passed, the unit is considered valid for loading
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