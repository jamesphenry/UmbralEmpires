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

    // --- CORRECTED REFACTORED IsValidUnit ---
    private bool IsValidUnit(UnitDefinition? unit)
    {
        // Perform checks sequentially. If any validation step fails, return false immediately.
        return unit != null &&
               ValidateUnitBasicProperties(unit) &&               // Check required strings, negative numbers etc.
               ValidateUnitRequiredShipyard(unit.RequiredShipyard) && // Check the shipyard record
               ValidateUnitRequiresTechnologyList(unit.RequiresTechnology) && // Check the tech list structure/items
               ValidateDriveTypeTechRequirements(unit);            // Check specific rules linking DriveType and TechReqs
    }

    // --- CORRECTED HELPER METHODS ---

    private bool ValidateUnitBasicProperties(UnitDefinition unit)
    {
        // Basic null/whitespace checks for required string properties
        if (string.IsNullOrWhiteSpace(unit.Id)) return false;
        if (string.IsNullOrWhiteSpace(unit.Name)) return false;
        // DriveType and WeaponType MUST be present for any valid unit definition
        if (string.IsNullOrWhiteSpace(unit.DriveType)) return false;
        if (string.IsNullOrWhiteSpace(unit.WeaponType)) return false;

        // Basic negative value checks
        if (unit.CreditsCost < 0) return false;
        if (unit.Attack < 0) return false;
        if (unit.Armour < 0) return false;
        if (unit.Shield < 0) return false;
        if (unit.Hangar < 0) return false;
        if (unit.Speed < 0) return false;

        return true; // All basic property checks passed
    }

    private bool ValidateUnitRequiredShipyard(ShipyardRequirement? req)
    {
        // Check the ShipyardRequirement itself and its levels
        if (req == null) return false;
        if (req.BaseLevel < 0) return false;
        if (req.OrbitalLevel < 0) return false;
        return true;
    }

    private bool ValidateUnitRequiresTechnologyList(List<TechRequirement>? techReqs)
    {
        // A null list is considered structurally valid here; specific checks happen later if needed by drive type.
        if (techReqs == null) return true;

        // Check individual requirements in the list
        foreach (var requirement in techReqs)
        {
            // Check for null item, invalid TechId, or invalid Level
            if (requirement == null || string.IsNullOrWhiteSpace(requirement.TechId) || requirement.Level <= 0)
            {
                return false; // Invalid item found
            }
        }

        // Check for duplicates using LINQ
        var hasDuplicates = techReqs
                            .GroupBy(r => r.TechId, StringComparer.OrdinalIgnoreCase)
                            .Any(g => g.Count() > 1);
        if (hasDuplicates)
        {
            return false; // Duplicates found
        }

        return true; // List structure and items are valid
    }

    // Renamed helper focuses ONLY on the rules linking drive type and tech reqs
    private bool ValidateDriveTypeTechRequirements(UnitDefinition unit)
    {
        // Assume DriveType string itself is already validated by ValidateUnitBasicProperties
        string driveType = unit.DriveType; // Should not be null/whitespace here
        List<TechRequirement>? techReqs = unit.RequiresTechnology;

        // Define valid drive types AGAIN here just for the specific checks below
        // NOTE: We could potentially pass the validated DriveType enum/value in, 
        // but this keeps helpers more self-contained for now.
        var validDriveTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Inter", "Stellar", "Warp" };

        // Belt-and-suspenders check, though ValidateUnitBasicProperties should have caught this
        if (!validDriveTypes.Contains(driveType))
        {
            // This case *shouldn't* be hit if ValidateUnitBasicProperties ran first,
            // but including for safety during refactoring.
            // Consider logging a warning if this state is reached.
            return false;
        }

        // Check specific tech requirements based on drive type
        if (driveType.Equals("Stellar", StringComparison.OrdinalIgnoreCase))
        {
            bool requiresStellar = techReqs?.Any(req =>
                req.TechId.Equals("Stellar Drive", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!requiresStellar) return false; // Missing required tech
        }
        else if (driveType.Equals("Warp", StringComparison.OrdinalIgnoreCase))
        {
            bool requiresWarp = techReqs?.Any(req =>
                req.TechId.Equals("Warp Drive", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!requiresWarp) return false; // Missing required tech
        }
        // No specific drive tech needed for "Inter" drive type

        return true; // Specific drive/tech rules passed (or didn't apply)
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