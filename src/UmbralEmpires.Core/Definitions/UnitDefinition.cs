// src/UmbralEmpires.Core/Definitions/UnitDefinition.cs
namespace UmbralEmpires.Core.Definitions; // File-scoped namespace

public record UnitDefinition
{
    public string Id { get; init; } = string.Empty; // Implicit ID based on name? Need to add to GDD/JSON? Using Name as ID for now? Let's assume JSON will have an ID field.
    public string Name { get; init; } = string.Empty;
    public int CreditsCost { get; init; }
    public string DriveType { get; init; } = string.Empty; // Could be enum later
    public string WeaponType { get; init; } = string.Empty; // Could be enum later
    public int Attack { get; init; }
    public int Armour { get; init; }
    public int Shield { get; init; }
    public int Hangar { get; init; }
    public int Speed { get; init; }
    public ShipyardRequirement RequiredShipyard { get; init; } = new(1); // Default to level 1 base?
    public List<TechRequirement> RequiresTechnology { get; init; } = new();
    // Description? Other stats?
}