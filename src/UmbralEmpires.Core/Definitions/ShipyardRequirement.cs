// src/UmbralEmpires.Core/Definitions/ShipyardRequirement.cs
namespace UmbralEmpires.Core.Definitions; // File-scoped namespace

public record ShipyardRequirement(int BaseLevel, int OrbitalLevel = 0); // Orbital defaults to 0
