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
            return new BaseModDefinitions();
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var loadedData = JsonSerializer.Deserialize<BaseModDefinitions>(jsonContent, options);

            if (loadedData == null)
            {
                Console.WriteLine("Warning: Deserialization resulted in null BaseModDefinitions object.");
                return new BaseModDefinitions();
            }

            // --- Existing Validation ---
            // Filter Structures
            var initialStructureCount = loadedData.Structures?.Count ?? 0;
            var validStructures = loadedData.Structures?
                                      .Where(IsValidStructure) // Now uses generic tech validation
                                      .ToList() ?? new List<StructureDefinition>();
            if (validStructures.Count < initialStructureCount)
                Console.WriteLine($"Warning: Skipped {initialStructureCount - validStructures.Count} structure(s) due to validation errors.");

            // Filter Technologies
            var initialTechCount = loadedData.Technologies?.Count ?? 0;
            var validTechnologies = loadedData.Technologies?
                                      .Where(IsValidTechnology) // Now uses generic tech validation
                                      .ToList() ?? new List<TechnologyDefinition>();
            if (validTechnologies.Count < initialTechCount)
                Console.WriteLine($"Warning: Skipped {initialTechCount - validTechnologies.Count} technology(s) due to validation errors.");

            // Filter Units
            var initialUnitCount = loadedData.Units?.Count ?? 0;
            var validUnits = loadedData.Units?
                                      .Where(IsValidUnit) // Now uses generic tech validation
                                      .ToList() ?? new List<UnitDefinition>();
            if (validUnits.Count < initialUnitCount)
                Console.WriteLine($"Warning: Skipped {initialUnitCount - validUnits.Count} unit(s) due to validation errors.");

            // --- NEW: Filter Defenses ---
            var initialDefenseCount = loadedData.Defenses?.Count ?? 0;
            var validDefenses = loadedData.Defenses?
                                      .Where(IsValidDefense) // Use the new validation method
                                      .ToList() ?? new List<DefenseDefinition>();
            if (validDefenses.Count < initialDefenseCount)
                Console.WriteLine($"Warning: Skipped {initialDefenseCount - validDefenses.Count} defense(s) due to validation errors.");
            // --- END NEW ---


            // Return validated data including defenses
            return loadedData with
            {
                Structures = validStructures,
                Technologies = validTechnologies,
                Units = validUnits,
                Defenses = validDefenses // <-- Add validated defenses here
            };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing base definitions: {ex.Message}");
            throw new InvalidOperationException("Failed to load base game definitions due to invalid JSON.", ex);
        }
    }

    // --- Generic Tech Requirement List Validation ---
    // (Can be used by Unit, Tech, Defense, Structure)
    private bool ValidateTechRequirementsList(List<TechRequirement>? techReqs, string parentId, string parentType)
    {
        Console.WriteLine($" ---> ValidateTechRequirementsList START for {parentType} {parentId}");
        if (techReqs == null)
        {
            Console.WriteLine($" ---> TechReqs list is null for {parentType} {parentId}, skipping checks - PASSED");
            return true; // An empty or null list is valid
        }

        int index = 0;
        foreach (var requirement in techReqs)
        {
            if (requirement == null) { Console.WriteLine($"FAILED: {parentType} {parentId} TechReqs item at index {index} is null"); return false; }
            if (string.IsNullOrWhiteSpace(requirement.TechId)) { Console.WriteLine($"FAILED: {parentType} {parentId} TechReqs item at index {index} has empty TechId"); return false; }
            if (requirement.Level <= 0) { Console.WriteLine($"FAILED: {parentType} {parentId} TechReqs item at index {index} has Level <= 0 (TechId: {requirement.TechId})"); return false; }
            index++;
        }

        // Check for duplicates
        var duplicateGroups = techReqs
                            .GroupBy(r => r.TechId, StringComparer.OrdinalIgnoreCase) // Case-insensitive check for TechId duplicates
                            .Where(g => g.Count() > 1)
                            .ToList();
        if (duplicateGroups.Any())
        {
            Console.WriteLine($"FAILED: {parentType} {parentId} found duplicate TechReqs TechIds: {string.Join(", ", duplicateGroups.Select(g => g.Key))}");
            return false;
        }

        Console.WriteLine($" ---> ValidateTechRequirementsList END for {parentType} {parentId}: PASSED");
        return true;
    }

    // --- IsValidStructure (Updated) ---
    private bool IsValidStructure(StructureDefinition? structure)
    {
        if (structure == null) return false;
        if (string.IsNullOrWhiteSpace(structure.Id)) return false;
        if (structure.BaseCreditsCost < 0) return false;
        if (string.IsNullOrWhiteSpace(structure.Name)) return false;
        // Add more basic property checks here if needed...

        // Use the generic validator for tech requirements
        if (!ValidateTechRequirementsList(structure.RequiresTechnology, structure.Id, "Structure")) return false;

        return true;
    }

    // --- IsValidTechnology (Updated) ---
    private bool IsValidTechnology(TechnologyDefinition? tech)
    {
        if (tech == null) return false;
        if (string.IsNullOrWhiteSpace(tech.Id)) return false;
        if (string.IsNullOrWhiteSpace(tech.Name)) return false;
        if (tech.CreditsCost < 0) return false;
        if (tech.RequiredLabsLevel < 0) return false; // Changed to < 0 for consistency with others

        // Use the generic validator for prerequisites
        if (!ValidateTechRequirementsList(tech.RequiresPrerequisites, tech.Id, "Technology")) return false;

        return true;
    }

    // --- IsValidUnit (Updated) ---
    private bool IsValidUnit(UnitDefinition? unit)
    {
        Console.WriteLine($"--- IsValidUnit START: Checking unit {unit?.Id ?? "NULL"} ---");
        if (unit == null)
        {
            Console.WriteLine("FAILED: Unit is null");
            return false;
        }

        bool basicValid = ValidateUnitBasicProperties(unit);
        Console.WriteLine($"--> Result ValidateUnitBasicProperties for {unit.Id}: {basicValid}");
        if (!basicValid) return false;

        bool shipyardValid = ValidateUnitRequiredShipyard(unit.RequiredShipyard);
        Console.WriteLine($"--> Result ValidateUnitRequiredShipyard for {unit.Id}: {shipyardValid}");
        if (!shipyardValid) return false;

        // *** USE GENERIC VALIDATOR ***
        bool techListValid = ValidateTechRequirementsList(unit.RequiresTechnology, unit.Id, "Unit");
        Console.WriteLine($"--> Result ValidateTechRequirementsList for {unit.Id}: {techListValid}");
        if (!techListValid) return false;

        bool driveTechValid = ValidateDriveTypeTechRequirements(unit);
        Console.WriteLine($"--> Result ValidateDriveTypeTechRequirements for {unit.Id}: {driveTechValid}");
        if (!driveTechValid) return false;

        Console.WriteLine($"--- IsValidUnit END: Unit {unit.Id} PASSED ---");
        return true;
    }

    // --- Unit Validation Helpers (Keep existing) ---
    private bool ValidateUnitBasicProperties(UnitDefinition unit)
    {
        Console.WriteLine($" ---> ValidateUnitBasicProperties START: Checking unit {unit.Id}");
        if (string.IsNullOrWhiteSpace(unit.Id)) { Console.WriteLine($"FAILED: Basic Id is empty for {unit.Id}"); return false; }
        if (string.IsNullOrWhiteSpace(unit.Name)) { Console.WriteLine($"FAILED: Basic Name is empty for {unit.Id}"); return false; }
        if (string.IsNullOrWhiteSpace(unit.DriveType)) { Console.WriteLine($"FAILED: Basic DriveType is empty for {unit.Id}"); return false; }
        if (string.IsNullOrWhiteSpace(unit.WeaponType)) { Console.WriteLine($"FAILED: Basic WeaponType is empty for {unit.Id}"); return false; }
        if (unit.CreditsCost < 0) { Console.WriteLine($"FAILED: Basic CreditsCost is < 0 for {unit.Id}"); return false; }
        if (unit.Attack < 0) { Console.WriteLine($"FAILED: Basic Attack is < 0 for {unit.Id}"); return false; }
        if (unit.Armour < 0) { Console.WriteLine($"FAILED: Basic Armour is < 0 for {unit.Id}"); return false; }
        if (unit.Shield < 0) { Console.WriteLine($"FAILED: Basic Shield is < 0 for {unit.Id}"); return false; }
        if (unit.Hangar < 0) { Console.WriteLine($"FAILED: Basic Hangar is < 0 for {unit.Id}"); return false; }
        if (unit.Speed < 0) { Console.WriteLine($"FAILED: Basic Speed is < 0 for {unit.Id}"); return false; }
        Console.WriteLine($" ---> ValidateUnitBasicProperties END: Unit {unit.Id} PASSED");
        return true;
    }

    private bool ValidateUnitRequiredShipyard(ShipyardRequirement? req)
    {
        Console.WriteLine($" ---> ValidateUnitRequiredShipyard START");
        if (req == null) { Console.WriteLine("FAILED: ShipyardRequirement is null"); return false; }
        if (req.BaseLevel < 0) { Console.WriteLine("FAILED: Shipyard BaseLevel < 0"); return false; }
        if (req.OrbitalLevel < 0) { Console.WriteLine("FAILED: Shipyard OrbitalLevel < 0"); return false; }
        Console.WriteLine($" ---> ValidateUnitRequiredShipyard END: PASSED");
        return true;
    }

    private bool ValidateDriveTypeTechRequirements(UnitDefinition unit)
    {
        Console.WriteLine($" ---> ValidateDriveTypeTechRequirements START: Unit {unit.Id}, Drive: {unit.DriveType}");
        string driveType = unit.DriveType;
        List<TechRequirement>? techReqs = unit.RequiresTechnology;

        var validDriveTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "Inter", "Stellar", "Warp" };

        // Basic check - should have been caught by ValidateUnitBasicProperties, but check again
        if (!validDriveTypes.Contains(driveType))
        {
            Console.WriteLine($"FAILED: Drive/Tech - Unknown DriveType '{driveType}' (Should not happen if BasicProperties passed)");
            return false;
        }

        if (driveType.Equals("Stellar", StringComparison.OrdinalIgnoreCase))
        {
            bool requiresStellar = techReqs?.Any(req =>
                req.TechId.Equals("Stellar Drive", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!requiresStellar) { Console.WriteLine($"FAILED: Drive/Tech - Stellar drive unit {unit.Id} missing 'Stellar Drive' tech req"); return false; }
            Console.WriteLine($" ---> Drive/Tech - Stellar drive requires Stellar Drive tech: PASSED");
        }
        else if (driveType.Equals("Warp", StringComparison.OrdinalIgnoreCase))
        {
            bool requiresWarp = techReqs?.Any(req =>
                req.TechId.Equals("Warp Drive", StringComparison.OrdinalIgnoreCase)) ?? false;
            if (!requiresWarp) { Console.WriteLine($"FAILED: Drive/Tech - Warp drive unit {unit.Id} missing 'Warp Drive' tech req"); return false; }
            Console.WriteLine($" ---> Drive/Tech - Warp drive requires Warp Drive tech: PASSED");
        }
        else // Inter
        {
            Console.WriteLine($" ---> Drive/Tech - Inter drive, no specific tech check needed: PASSED");
        }

        // Note: WeaponType check moved back to ValidateUnitBasicProperties

        Console.WriteLine($" ---> ValidateDriveTypeTechRequirements END: Unit {unit.Id} PASSED");
        return true;
    }

    // --- NEW: IsValidDefense and Helpers ---
    private bool IsValidDefense(DefenseDefinition? defense)
    {
        // Basic null check
        if (defense == null)
        {
            Console.WriteLine("FAILED: Defense is null");
            return false;
        }

        // Validate basic properties
        if (!ValidateDefenseBasicProperties(defense)) return false;

        // Validate tech requirements list using the generic helper
        if (!ValidateTechRequirementsList(defense.RequiresTechnology, defense.Id, "Defense")) return false;

        Console.WriteLine($"--- IsValidDefense END: Defense {defense.Id} PASSED ---");
        return true;
    }

    private bool ValidateDefenseBasicProperties(DefenseDefinition defense)
    {
        Console.WriteLine($" ---> ValidateDefenseBasicProperties START: Checking defense {defense.Id}");
        if (string.IsNullOrWhiteSpace(defense.Id)) { Console.WriteLine($"FAILED: Basic Id is empty for Defense {defense.Id}"); return false; }
        if (string.IsNullOrWhiteSpace(defense.Name)) { Console.WriteLine($"FAILED: Basic Name is empty for Defense {defense.Id}"); return false; }
        if (string.IsNullOrWhiteSpace(defense.WeaponType)) { Console.WriteLine($"FAILED: Basic WeaponType is empty for Defense {defense.Id}"); return false; } // WeaponType is required
        if (defense.BaseCreditsCost < 0) { Console.WriteLine($"FAILED: Basic BaseCreditsCost is < 0 for Defense {defense.Id}"); return false; }
        if (defense.Attack < 0) { Console.WriteLine($"FAILED: Basic Attack is < 0 for Defense {defense.Id}"); return false; }
        if (defense.Armour < 0) { Console.WriteLine($"FAILED: Basic Armour is < 0 for Defense {defense.Id}"); return false; }
        if (defense.Shield < 0) { Console.WriteLine($"FAILED: Basic Shield is < 0 for Defense {defense.Id}"); return false; }
        if (defense.EnergyRequirementPerLevel < 0) { Console.WriteLine($"FAILED: Basic EnergyRequirementPerLevel is < 0 for Defense {defense.Id}"); return false; }
        if (defense.PopulationRequirementPerLevel < 0) { Console.WriteLine($"FAILED: Basic PopulationRequirementPerLevel is < 0 for Defense {defense.Id}"); return false; }
        if (defense.AreaRequirementPerLevel < 0) { Console.WriteLine($"FAILED: Basic AreaRequirementPerLevel is < 0 for Defense {defense.Id}"); return false; }

        Console.WriteLine($" ---> ValidateDefenseBasicProperties END: Defense {defense.Id} PASSED");
        return true;
    }

    // NOTE: The old 'ValidateUnitRequiresTechnologyList' method should be removed if it still exists.
    // The generic 'ValidateTechRequirementsList' replaces it.
}