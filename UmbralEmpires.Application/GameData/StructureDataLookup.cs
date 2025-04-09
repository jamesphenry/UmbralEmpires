using UmbralEmpires.Core.Gameplay; // For StructureType enum
using System;
using System.Collections.Generic;

namespace UmbralEmpires.Application.GameData;

public static class StructureDataLookup
{
    // StructureType -> (BaseCreditCost Lvl1, EnergyReq PerLvl, PopReq PerLvl, AreaReq PerLvl)
    private static readonly Dictionary<StructureType, (int BC, int E, int P, int A)> BaseData = new()
    {
        // === Structures (GDD 6.2) ===
        // Energy marked '-' assumed 0 req. Pop/Area marked '-' or N/A assumed 0 req.
        { StructureType.UrbanStructures,       (BC: 1,    E: 0, P: 1, A: 1) }, // P contributes to MaxPop via Fertility calc
        { StructureType.SolarPlants,         (BC: 1,    E: 0, P: 0, A: 1) }, // Produces E
        { StructureType.GasPlants,           (BC: 1,    E: 0, P: 0, A: 1) }, // Produces E
        { StructureType.FusionPlants,        (BC: 20,   E: 4, P: 0, A: 1) }, // Requires E:4/lvl, Produces E
        { StructureType.AntimatterPlants,    (BC: 2000, E: 10,P: 0, A: 1) }, // Requires E:10/lvl, Produces E
        { StructureType.OrbitalPlants,       (BC: 40000,E: 12,P: 0, A: 0) }, // Requires E:12/lvl, Produces E
        { StructureType.ResearchLabs,        (BC: 2,    E: 0, P: 1, A: 1) },
        { StructureType.MetalRefineries,     (BC: 1,    E: 0, P: 1, A: 1) },
        { StructureType.CrystalMines,        (BC: 2,    E: 0, P: 1, A: 1) },
        { StructureType.RoboticFactories,    (BC: 5,    E: 0, P: 1, A: 1) },
        { StructureType.Shipyards,           (BC: 5,    E: 0, P: 1, A: 1) },
        { StructureType.OrbitalShipyards,    (BC: 10000,E: 0, P: 0, A: 1) }, // Pop req 0
        { StructureType.Spaceports,          (BC: 5,    E: 0, P: 1, A: 1) },
        { StructureType.CommandCenters,      (BC: 20,   E: 0, P: 1, A: 1) },
        { StructureType.NaniteFactories,     (BC: 80,   E: 0, P: 1, A: 1) }, // E req 0
        { StructureType.AndroidFactories,    (BC: 1000, E: 0, P: 1, A: 1) }, // E req 0
        { StructureType.EconomicCenters,     (BC: 80,   E: 0, P: 1, A: 1) }, // E req 0
        { StructureType.Terraform,           (BC: 80,   E: 0, P: 0, A: 0) }, // E/P/A req 0, Increases MaxArea
        { StructureType.MultiLevelPlatforms, (BC: 10000,E: 0, P: 0, A: 0) }, // E/P/A req 0, Increases MaxArea
        { StructureType.OrbitalBase,         (BC: 2000, E: 0, P: 0, A: 0) }, // E/P/A req 0, Increases MaxPop
        { StructureType.JumpGate,            (BC: 5000, E: 0, P: 0, A: 1) }, // E/P req 0
        { StructureType.BiosphereModification,(BC: 20000,E: 0, P: 0, A: 0) }, // E/P/A req 0, Increases Fertility
        { StructureType.Capital,             (BC: 15000,E: 0, P: 1, A: 1) }, // E req 0

        // === Defenses (GDD 6.4) ===
        // Note: All Defenses require 1 Pop per level per GDD note, except shields/ring.
        { StructureType.Barracks,            (BC: 5,   E: 0, P: 1, A: 1) },
        { StructureType.LaserTurrets,        (BC: 10,  E: 0, P: 1, A: 1) },
        { StructureType.MissileTurrets,      (BC: 20,  E: 1, P: 1, A: 1) },
        { StructureType.PlasmaTurrets,       (BC: 100, E: 2, P: 1, A: 1) },
        { StructureType.IonTurrets,          (BC: 250, E: 3, P: 1, A: 1) },
        { StructureType.PhotonTurrets,       (BC: 1000,E: 6, P: 1, A: 1) }, // Using GDD E:6
        { StructureType.DisruptorTurrets,    (BC: 4000,E: 8, P: 1, A: 1) },
        { StructureType.DeflectionShields,   (BC: 4000,E: 16,P: 0, A: 0) }, // Pop/Area req 0
        { StructureType.PlanetaryShield,     (BC: 25000,E: 20,P: 0, A: 0) }, // Pop/Area req 0
        { StructureType.PlanetaryRing,       (BC: 50000,E: 24,P: 0, A: 0) }, // Pop/Area req 0
    };

    private const double CreditCostMultiplier = 1.5;

    /// <summary> Calculates Credit cost for building a specific level L </summary>
    public static int GetCreditCostForLevel(StructureType type, int level)
    {
        if (level <= 0) return 0;
        if (BaseData.TryGetValue(type, out var data) && data.BC > 0)
        {
            double cost = data.BC * Math.Pow(CreditCostMultiplier, level - 1);
            return (int)Math.Max(1, Math.Floor(cost)); // Cost is at least 1 if BaseCost > 0
        }
        // Log error ideally using a proper logging mechanism if type not found
        Console.WriteLine($"ERROR: Structure type {type} not found for cost calculation."); // Simple fallback log
        return 0;
    }

    /// <summary> Gets the constant Energy required per level </summary>
    public static int GetEnergyRequirementPerLevel(StructureType type)
    {
        return BaseData.TryGetValue(type, out var data) ? data.E : 0;
    }

    /// <summary> Gets the constant Population required per level </summary>
    public static int GetPopulationRequirementPerLevel(StructureType type)
    {
        // Returns 0 for structures explicitly listed with no Pop req in GDD
        return BaseData.TryGetValue(type, out var data) ? data.P : 0;
    }

    /// <summary> Gets the constant Area required per level </summary>
    public static int GetAreaRequirementPerLevel(StructureType type)
    {
        // Returns 0 for structures explicitly listed with no Area req in GDD
        return BaseData.TryGetValue(type, out var data) ? data.A : 0;
    }

    /// <summary> Calculates TOTAL E/P/A required for ALL levels up to the given level </summary>
    // Total Requirement = RequirementPerLevel * CurrentLevel
    public static int GetTotalEnergyCost(StructureType type, int level) => GetEnergyRequirementPerLevel(type) * level;
    public static int GetTotalPopulationCost(StructureType type, int level) => GetPopulationRequirementPerLevel(type) * level;
    public static int GetTotalAreaCost(StructureType type, int level) => GetAreaRequirementPerLevel(type) * level;

    // --- TODO: Add methods for other static data as needed ---
    // e.g., GetStructurePrerequisites(StructureType type), GetMaxLevel(StructureType type)
}