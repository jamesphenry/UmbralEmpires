﻿// src/UmbralEmpires.Core/Definitions/StructureDefinition.cs
namespace UmbralEmpires.Core.Definitions; // File-scoped namespace

// Using a record for concise immutable data structure
public record StructureDefinition
{
    public string Id { get; init; } = string.Empty; 
    public string Name { get; init; } = string.Empty;
    public int BaseCreditsCost { get; init; }
    public int EnergyRequirementPerLevel { get; init; }
    public int PopulationRequirementPerLevel { get; init; }
    public int AreaRequirementPerLevel { get; init; }
    public List<TechRequirement> RequiresTechnology { get; init; } = new();
    public int EconomyBonus { get; init; }
    public bool IsAdvanced { get; init; }
    public int BaseConstructionBonus { get; init; }
    public int BaseProductionBonus { get; init; }
    public int BaseResearchBonus { get; init; }
    public bool UsesMetal { get; init; }
    public bool UsesGas { get; init; }
    public bool UsesCrystal { get; init; }
    public bool UsesSolar { get; init; }
    public bool AddsPopCapacityByFertility { get; init; }
    public int AreaCapacityBonus { get; init; }
    public bool IncreasesAstroFertility { get; init; }
}
