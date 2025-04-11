// src/UmbralEmpires.Core/Definitions/StructureDefinition.cs
namespace UmbralEmpires.Core.Definitions; // File-scoped namespace

// Using a record for concise immutable data structure
public record StructureDefinition
{
    // Only include properties needed for the first test
    public string Id { get; init; } = string.Empty; // Initialize to avoid warnings
    public string Name { get; init; } = string.Empty;
    public int BaseCreditsCost { get; init; }
    public int EnergyRequirementPerLevel { get; init; }
    public int PopulationRequirementPerLevel { get; init; }
    public int AreaRequirementPerLevel { get; init; }

    // We'll add other properties (Requirements, Bonuses, etc.) in later TDD cycles
}