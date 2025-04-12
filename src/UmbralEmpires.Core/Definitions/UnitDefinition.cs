// src/UmbralEmpires.Core/Definitions/UnitDefinition.cs (New File)
using System.Collections.Generic;
namespace UmbralEmpires.Core.Definitions;

// Represents Shipyard requirements (Base Level, Optional Orbital Level)
// Already defined conceptually when discussing structures/techs, ensure it exists
// public record ShipyardRequirement(int BaseLevel, int OrbitalLevel = 0);

public record UnitDefinition
{
    public string Id { get; init; } = string.Empty; // Added Id field for consistency
    public string Name { get; init; } = string.Empty;
    public int CreditsCost { get; init; }
    public string DriveType { get; init; } = string.Empty; // e.g., "Interceptor", "Stellar", "Warp" - Could be enum later
    public string WeaponType { get; init; } = string.Empty; // e.g., "Laser", "Missiles", "Plasma" - Could be enum later
    public int Attack { get; init; }
    public int Armour { get; init; }
    public int Shield { get; init; }
    public int Hangar { get; init; } // Hangar capacity (for carriers)
    public int Speed { get; init; }
    public ShipyardRequirement RequiredShipyard { get; init; } = new(1, 0); // Default to base level 1?
    public List<TechRequirement> RequiresTechnology { get; init; } = new();
    public string Description { get; init; } = string.Empty; // Add description field? GDD table has one.
}