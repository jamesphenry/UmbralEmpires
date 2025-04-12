namespace UmbralEmpires.Core.Definitions;

public record DefenseDefinition
{
    public string Id { get; init; } = string.Empty;        // Added for identification
    public string Name { get; init; } = string.Empty;
    public int BaseCreditsCost { get; init; }              // Assuming cost scales like structures
    public string WeaponType { get; init; } = string.Empty; // Could be enum later
    public int Attack { get; init; }
    public int Armour { get; init; }
    public int Shield { get; init; }
    public int EnergyRequirementPerLevel { get; init; }
    public int PopulationRequirementPerLevel { get; init; } = 1; // GDD says 1 per level
    public int AreaRequirementPerLevel { get; init; }        // GDD implies 1 or 0, needs confirmation? Defaulting to 1 for turrets, 0 for shields/ring?
    public List<TechRequirement> RequiresTechnology { get; init; } = new();
    // Add Description if needed later
}