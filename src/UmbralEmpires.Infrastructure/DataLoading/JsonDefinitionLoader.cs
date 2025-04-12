// src/UmbralEmpires.Infrastructure/DataLoading/JsonDefinitionLoader.cs
using System;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ
using System.Text.Json;
using UmbralEmpires.Application.Interfaces;
using UmbralEmpires.Core.Definitions;

namespace UmbralEmpires.Infrastructure.DataLoading;

public class JsonDefinitionLoader : IDefinitionLoader
{
    // --- LoadAllDefinitions method ---
    public BaseModDefinitions LoadAllDefinitions(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            Console.WriteLine("Warning: Definition content is empty, returning empty definitions.");
            return new BaseModDefinitions(); // Return empty object
        }

        // Temporary storage for definitions passing basic validation, using case-insensitive keys
        var tempStructures = new Dictionary<string, StructureDefinition>(StringComparer.OrdinalIgnoreCase);
        var tempTechnologies = new Dictionary<string, TechnologyDefinition>(StringComparer.OrdinalIgnoreCase);
        var tempUnits = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);
        var tempDefenses = new Dictionary<string, DefenseDefinition>(StringComparer.OrdinalIgnoreCase);

        // Track IDs that fail validation (either basic or duplicate ID)
        var invalidIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        BaseModDefinitions? loadedData;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            loadedData = JsonSerializer.Deserialize<BaseModDefinitions>(jsonContent, options);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing base definitions: {ex.Message}");
            throw new InvalidOperationException("Failed to load base game definitions due to invalid JSON.", ex);
        }

        if (loadedData == null)
        {
            Console.WriteLine("Warning: Deserialization resulted in null BaseModDefinitions object.");
            return new BaseModDefinitions(); // Return empty object
        }

        // --- Pass 1: Basic Validation and Indexing ---
        Console.WriteLine("--- Starting Pass 1: Basic Validation & Indexing ---");
        ProcessDefinitionsPass1(loadedData.Structures, tempStructures, invalidIds, IsValidStructureBasic);
        ProcessDefinitionsPass1(loadedData.Technologies, tempTechnologies, invalidIds, IsValidTechnologyBasic);
        ProcessDefinitionsPass1(loadedData.Units, tempUnits, invalidIds, IsValidUnitBasic);
        ProcessDefinitionsPass1(loadedData.Defenses, tempDefenses, invalidIds, IsValidDefenseBasic);
        Console.WriteLine($"--- Pass 1 Complete: Found {invalidIds.Count} invalid/duplicate basic definitions. ---");


        // --- Pass 2: Cross-Reference Validation ---
        Console.WriteLine("--- Starting Pass 2: Cross-Reference Validation ---");
        var crossRefFailedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Validate Technology Prerequisites
        foreach (var kvp in tempTechnologies)
        {
            if (invalidIds.Contains(kvp.Key)) continue; // Skip already invalid items
            if (!ValidateRequirements(kvp.Value.RequiresPrerequisites, kvp.Key, "TechnologyPrereq", tempTechnologies, invalidIds))
            {
                crossRefFailedIds.Add(kvp.Key);
            }
        }

        // Validate Structure Tech Requirements
        foreach (var kvp in tempStructures)
        {
            if (invalidIds.Contains(kvp.Key)) continue;
            if (!ValidateRequirements(kvp.Value.RequiresTechnology, kvp.Key, "StructureTech", tempTechnologies, invalidIds))
            {
                crossRefFailedIds.Add(kvp.Key);
            }
        }

        // Validate Unit Tech Requirements (including Drive checks)
        foreach (var kvp in tempUnits)
        {
            if (invalidIds.Contains(kvp.Key)) continue;
            if (!ValidateRequirements(kvp.Value.RequiresTechnology, kvp.Key, "UnitTech", tempTechnologies, invalidIds) ||
                !ValidateUnitDriveTechExists(kvp.Value, tempTechnologies, invalidIds)) // Pass dependencies
            {
                crossRefFailedIds.Add(kvp.Key);
            }
        }

        // Validate Defense Tech Requirements
        foreach (var kvp in tempDefenses)
        {
            if (invalidIds.Contains(kvp.Key)) continue;
            if (!ValidateRequirements(kvp.Value.RequiresTechnology, kvp.Key, "DefenseTech", tempTechnologies, invalidIds))
            {
                crossRefFailedIds.Add(kvp.Key);
            }
        }
        Console.WriteLine($"--- Pass 2 Complete: Found {crossRefFailedIds.Count} definitions failing cross-reference checks. ---");

        // Combine invalid IDs
        invalidIds.UnionWith(crossRefFailedIds);
        Console.WriteLine($"--- Total Invalid/Skipped Definitions: {invalidIds.Count} ---");


        // --- Final Filtering & Return ---
        var finalStructures = tempStructures.Values.Where(d => !invalidIds.Contains(d.Id)).ToList();
        var finalTechnologies = tempTechnologies.Values.Where(d => !invalidIds.Contains(d.Id)).ToList();
        var finalUnits = tempUnits.Values.Where(d => !invalidIds.Contains(d.Id)).ToList();
        var finalDefenses = tempDefenses.Values.Where(d => !invalidIds.Contains(d.Id)).ToList();

        Console.WriteLine($"--- Loading Complete. Returning: " +
                          $"{finalStructures.Count} Structures, " +
                          $"{finalTechnologies.Count} Technologies, " +
                          $"{finalUnits.Count} Units, " +
                          $"{finalDefenses.Count} Defenses ---");

        return new BaseModDefinitions
        {
            Structures = finalStructures,
            Technologies = finalTechnologies,
            Units = finalUnits,
            Defenses = finalDefenses
        };
    }

    // --- Pass 1 Processing Helper ---
    private void ProcessDefinitionsPass1<TDef>(
        List<TDef>? definitions,
        Dictionary<string, TDef> tempDictionary,
        HashSet<string> invalidIds,
        Func<TDef?, bool> basicValidator) where TDef : class
    {
        if (definitions == null) return;

        foreach (var definition in definitions)
        {
            // Assume TDef has an 'Id' property (requires constraints or reflection if not guaranteed)
            // For simplicity, we'll assume an interface or direct property access
            string? id = (definition as dynamic)?.Id; // Simple dynamic access, replace with interface/reflection if needed

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine($"Warning: Skipping definition of type {typeof(TDef).Name} with missing or empty ID.");
                continue; // Cannot process without an ID
            }

            if (!basicValidator(definition))
            {
                Console.WriteLine($"--> Pass 1: Basic validation FAILED for {typeof(TDef).Name} '{id}'.");
                invalidIds.Add(id);
            }
            else
            {
                // Try adding to temp dictionary, checking for duplicates (case-insensitive)
                if (!tempDictionary.TryAdd(id, definition))
                {
                    Console.WriteLine($"--> Pass 1: Duplicate ID (case-insensitive) detected for {typeof(TDef).Name}. Skipping duplicate entry for '{id}'.");
                    invalidIds.Add(id); // Mark the ID itself as problematic due to duplication
                }
                else
                {
                    // Console.WriteLine($"--> Pass 1: Basic validation PASSED for {typeof(TDef).Name} '{id}'. Added to temp index.");
                }
            }
        }
    }

    // --- Pass 2 Cross-Reference Helper ---
    private bool ValidateRequirements(
       List<TechRequirement>? requirements,
       string parentId,
       string parentType,
       Dictionary<string, TechnologyDefinition> validTechDict, // Pass dict for lookups
       HashSet<string> initiallyInvalidIds) // Pass IDs that failed Pass 1
    {
        if (requirements == null || !requirements.Any())
        {
            return true; // No requirements to validate
        }

        foreach (var req in requirements)
        {
            if (req == null) // Should have been caught by basic validation, but double-check
            {
                Console.WriteLine($"--> Pass 2: {parentType} '{parentId}' FAILED cross-ref check: Contains null requirement entry.");
                return false;
            }

            // Check if the required Tech ID exists in the dictionary of basically valid techs
            if (!validTechDict.ContainsKey(req.TechId))
            {
                Console.WriteLine($"--> Pass 2: {parentType} '{parentId}' FAILED cross-ref check: Required TechId '{req.TechId}' not found or was invalid in Pass 1.");
                return false;
            }
            // Optional: Could also check if req.TechId exists in the initiallyInvalidIds set for a more specific message,
            // but ContainsKey check covers both missing and initially invalid cases.
        }
        // Console.WriteLine($"--> Pass 2: {parentType} '{parentId}' PASSED cross-ref check.");
        return true;
    }

    // --- Pass 2 Specific Check for Unit Drive Tech Existence ---
    // Refactored to check against the validated tech dictionary
    private bool ValidateUnitDriveTechExists(
        UnitDefinition unit,
        Dictionary<string, TechnologyDefinition> validTechDict,
        HashSet<string> initiallyInvalidIds)
    {
        string driveType = unit.DriveType;
        string requiredTechId = "";

        if (driveType.Equals("Stellar", StringComparison.OrdinalIgnoreCase))
        {
            requiredTechId = "Stellar Drive";
        }
        else if (driveType.Equals("Warp", StringComparison.OrdinalIgnoreCase))
        {
            requiredTechId = "Warp Drive";
        }
        else
        {
            return true; // Inter drive needs no specific tech check here
        }

        // Does the unit *list* this required tech? (Case-insensitive check within the list)
        bool listsRequirement = unit.RequiresTechnology?.Any(req =>
                req.TechId.Equals(requiredTechId, StringComparison.OrdinalIgnoreCase)) ?? false;

        if (!listsRequirement)
        {
            // This check remains, ensuring the unit *claims* to require the correct drive tech.
            // The cross-ref validation will then ensure that listed tech actually exists and is valid.
            Console.WriteLine($"--> Pass 2: Unit '{unit.Id}' with DriveType '{driveType}' FAILED check: Does not list required TechId '{requiredTechId}' in RequiresTechnology.");
            return false;
        }

        // We already validated the *format* of the requirements list in Pass 1.
        // Now, ensure the *specific* required drive tech ID exists and was valid in Pass 1.
        if (!validTechDict.ContainsKey(requiredTechId))
        {
            Console.WriteLine($"--> Pass 2: Unit '{unit.Id}' FAILED drive check: Required TechId '{requiredTechId}' not found among valid technologies or was invalid in Pass 1.");
            return false;
        }

        // Console.WriteLine($"--> Pass 2: Unit '{unit.Id}' PASSED drive tech existence check for '{requiredTechId}'.");
        return true;
    }


    // --- Basic Validation Methods (Pass 1 Checks) ---
    // These now ONLY check basic properties, not requirements yet.

    private bool IsValidStructureBasic(StructureDefinition? structure)
    {
        if (structure == null) return false;
        if (string.IsNullOrWhiteSpace(structure.Id)) return false;
        if (structure.BaseCreditsCost < 0) return false;
        if (string.IsNullOrWhiteSpace(structure.Name)) return false;
        // Check other value constraints (negative energy/pop/area etc.)
        if (structure.EnergyRequirementPerLevel < 0) return false;
        if (structure.PopulationRequirementPerLevel < 0) return false;
        if (structure.AreaRequirementPerLevel < 0) return false;
        // Check tech requirement list format ONLY (null entries, empty IDs, non-positive levels)
        if (!ValidateBasicTechReqListFormat(structure.RequiresTechnology)) return false;
        return true;
    }

    private bool IsValidTechnologyBasic(TechnologyDefinition? tech)
    {
        if (tech == null) return false;
        if (string.IsNullOrWhiteSpace(tech.Id)) return false;
        if (string.IsNullOrWhiteSpace(tech.Name)) return false;
        if (tech.CreditsCost < 0) return false;
        if (tech.RequiredLabsLevel < 0) return false; // Allow 0? GDD has Level 1 min. Test checks <0.
        // Check prerequisite list format ONLY
        if (!ValidateBasicTechReqListFormat(tech.RequiresPrerequisites)) return false;
        return true;
    }

    private bool IsValidUnitBasic(UnitDefinition? unit)
    {
        if (unit == null) return false;
        // Check basic props (reuse existing helper)
        if (!ValidateUnitBasicProperties(unit)) return false;
        // Check shipyard req format
        if (!ValidateUnitRequiredShipyard(unit.RequiredShipyard)) return false;
        // Check tech requirement list format ONLY
        if (!ValidateBasicTechReqListFormat(unit.RequiresTechnology)) return false;
        // Check drive type is known ("Inter", "Stellar", "Warp")
        if (!new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Inter", "Stellar", "Warp" }.Contains(unit.DriveType)) return false;
        return true;
    }

    private bool IsValidDefenseBasic(DefenseDefinition? defense)
    {
        if (defense == null) return false;
        // Check basic props (reuse existing helper)
        if (!ValidateDefenseBasicProperties(defense)) return false;
        // Check tech requirement list format ONLY
        if (!ValidateBasicTechReqListFormat(defense.RequiresTechnology)) return false;
        return true;
    }

    // Helper to validate only the format of a TechRequirement list (used in Pass 1)
    // Does NOT check for duplicates here, only format/nulls. Duplicates handled by dictionary add.
    private bool ValidateBasicTechReqListFormat(List<TechRequirement>? reqs)
    {
        if (reqs == null) return true; // Null list is valid format-wise

        foreach (var req in reqs)
        {
            if (req == null) return false; // Contains null entry
            if (string.IsNullOrWhiteSpace(req.TechId)) return false; // Empty TechId
            if (req.Level <= 0) return false; // Non-positive level
        }
        return true;
    }

    // --- Keep specific Unit/Defense property validation helpers ---
    private bool ValidateUnitBasicProperties(UnitDefinition unit)
    {
        // (Content remains the same as previous version)
        if (string.IsNullOrWhiteSpace(unit.Id)) return false;
        if (string.IsNullOrWhiteSpace(unit.Name)) return false;
        if (string.IsNullOrWhiteSpace(unit.DriveType)) return false;
        if (string.IsNullOrWhiteSpace(unit.WeaponType)) return false;
        if (unit.CreditsCost < 0) return false;
        if (unit.Attack < 0) return false;
        if (unit.Armour < 0) return false;
        if (unit.Shield < 0) return false;
        if (unit.Hangar < 0) return false;
        if (unit.Speed < 0) return false;
        return true;
    }

    private bool ValidateUnitRequiredShipyard(ShipyardRequirement? req)
    {
        // (Content remains the same as previous version)
        if (req == null) return false;
        if (req.BaseLevel < 0) return false;
        if (req.OrbitalLevel < 0) return false;
        return true;
    }

    private bool ValidateDefenseBasicProperties(DefenseDefinition defense)
    {
        // (Content remains the same as previous version)
        if (string.IsNullOrWhiteSpace(defense.Id)) return false;
        if (string.IsNullOrWhiteSpace(defense.Name)) return false;
        if (string.IsNullOrWhiteSpace(defense.WeaponType)) return false;
        if (defense.BaseCreditsCost < 0) return false;
        if (defense.Attack < 0) return false;
        if (defense.Armour < 0) return false;
        if (defense.Shield < 0) return false;
        if (defense.EnergyRequirementPerLevel < 0) return false;
        if (defense.PopulationRequirementPerLevel < 0) return false;
        if (defense.AreaRequirementPerLevel < 0) return false;
        return true;
    }

    // NOTE: The old generic 'ValidateTechRequirementsList' (which checked duplicates) and
    // specific `ValidateDriveTypeTechRequirements` are removed/replaced by the new structure.
}