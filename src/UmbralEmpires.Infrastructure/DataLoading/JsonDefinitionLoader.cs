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

            // Filter Structures
            var initialStructureCount = loadedData.Structures?.Count ?? 0;
            var validStructures = loadedData.Structures?
                                      .Where(IsValidStructure)
                                      .ToList() ?? new List<StructureDefinition>();
            if (validStructures.Count < initialStructureCount)
                Console.WriteLine($"Warning: Skipped {initialStructureCount - validStructures.Count} structure(s) due to validation errors.");

            // Filter Technologies
            var initialTechCount = loadedData.Technologies?.Count ?? 0;
            var validTechnologies = loadedData.Technologies?
                                      .Where(IsValidTechnology)
                                      .ToList() ?? new List<TechnologyDefinition>();
            if (validTechnologies.Count < initialTechCount)
                Console.WriteLine($"Warning: Skipped {initialTechCount - validTechnologies.Count} technology(s) due to validation errors.");

            // Filter Units
            var initialUnitCount = loadedData.Units?.Count ?? 0;
            var validUnits = loadedData.Units?
                                      .Where(IsValidUnit) // Use the refactored validator
                                      .ToList() ?? new List<UnitDefinition>();
            if (validUnits.Count < initialUnitCount)
                Console.WriteLine($"Warning: Skipped {initialUnitCount - validUnits.Count} unit(s) due to validation errors.");

            // Return validated data
            return loadedData with
            {
                Structures = validStructures,
                Technologies = validTechnologies,
                Units = validUnits
            };
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing base definitions: {ex.Message}");
            throw new InvalidOperationException("Failed to load base game definitions due to invalid JSON.", ex);
        }
    }

    // --- IsValidUnit and Helpers with Debug Logging ---

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

        bool techListValid = ValidateUnitRequiresTechnologyList(unit.RequiresTechnology);
        Console.WriteLine($"--> Result ValidateUnitRequiresTechnologyList for {unit.Id}: {techListValid}");
        if (!techListValid) return false;

        bool driveTechValid = ValidateDriveTypeTechRequirements(unit);
        Console.WriteLine($"--> Result ValidateDriveTypeTechRequirements for {unit.Id}: {driveTechValid}");
        if (!driveTechValid) return false;

        Console.WriteLine($"--- IsValidUnit END: Unit {unit.Id} PASSED ---");
        return true;
    }

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

    private bool ValidateUnitRequiresTechnologyList(List<TechRequirement>? techReqs)
    {
        Console.WriteLine($" ---> ValidateUnitRequiresTechnologyList START");
        if (techReqs == null)
        {
            Console.WriteLine($" ---> TechReqs list is null, skipping checks - PASSED");
            return true;
        }

        int index = 0;
        foreach (var requirement in techReqs)
        {
            if (requirement == null) { Console.WriteLine($"FAILED: TechReqs item at index {index} is null"); return false; }
            if (string.IsNullOrWhiteSpace(requirement.TechId)) { Console.WriteLine($"FAILED: TechReqs item at index {index} has empty TechId"); return false; }
            if (requirement.Level <= 0) { Console.WriteLine($"FAILED: TechReqs item at index {index} has Level <= 0 (TechId: {requirement.TechId})"); return false; }
            index++;
        }

        var duplicateGroups = techReqs
                            .GroupBy(r => r.TechId, StringComparer.OrdinalIgnoreCase)
                            .Where(g => g.Count() > 1)
                            .ToList();
        if (duplicateGroups.Any())
        {
            Console.WriteLine($"FAILED: Found duplicate TechReqs TechIds: {string.Join(", ", duplicateGroups.Select(g => g.Key))}");
            return false;
        }

        Console.WriteLine($" ---> ValidateUnitRequiresTechnologyList END: PASSED");
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

    // --- Existing IsValidStructure and IsValidTechnology methods ---
    // (These should remain as they were provided by you previously)
    private bool IsValidStructure(StructureDefinition? structure)
    {
        if (structure == null) return false;
        if (string.IsNullOrWhiteSpace(structure.Id)) return false;
        if (structure.BaseCreditsCost < 0) return false;
        if (string.IsNullOrWhiteSpace(structure.Name)) return false;
        // Add more checks here...
        return true;
    }

    private bool IsValidTechnology(TechnologyDefinition? tech)
    {
        if (tech == null) return false;
        if (string.IsNullOrWhiteSpace(tech.Id)) return false;
        if (string.IsNullOrWhiteSpace(tech.Name)) return false;
        if (tech.CreditsCost < 0) return false;
        if (tech.RequiredLabsLevel < 0) return false;

        if (tech.RequiresPrerequisites != null)
        {
            foreach (var requirement in tech.RequiresPrerequisites)
            {
                if (requirement == null) return false;
                if (string.IsNullOrWhiteSpace(requirement.TechId)) return false;
                if (requirement.Level <= 0) return false;
            }

            var hasDuplicates = tech.RequiresPrerequisites
                                    .GroupBy(r => r.TechId)
                                    .Any(g => g.Count() > 1);
            if (hasDuplicates)
            {
                Console.WriteLine($"Warning: Skipping tech ID '{tech.Id}' due to duplicate prerequisite TechIds.");
                return false;
            }
        }
        return true;
    }
}