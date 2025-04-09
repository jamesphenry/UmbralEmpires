using UmbralEmpires.Core.Gameplay;         // For Base, StructureType
using UmbralEmpires.Core.World;            // For Astro
using UmbralEmpires.Application.GameData;  // For StructureDataLookup
using System;                             // For Math.Floor
using System.Linq;                        // Potentially needed later

namespace UmbralEmpires.Application.Services;

// --- Implementation Class ---
public class BaseStatsCalculatorService : IBaseStatsCalculatorService
{
    // Dependencies can be injected via constructor later if needed
    // e.g., constructor(IPlayerTechRepository techRepo, IStructureDataProvider dataProvider)

    /// <summary>
    /// Calculates the derived statistics for a given Base based on its structures
    /// and the properties of the Astro it's built on.
    /// Assumes Tech Levels (Cybernetics, AI) are 0 for Milestone 1.
    /// </summary>
    /// <param name="playerBase">The Base entity containing structure levels.</param>
    /// <param name="astro">The Astro entity containing potentials.</param>
    /// <returns>A record containing all calculated stats.</returns>
    public CalculatedBaseStats CalculateStats(Base playerBase, Astro astro)
    {
        // --- Assumptions for Milestone 1 ---
        // Fetching actual tech levels would happen here in later milestones
        int cyberneticsLevel = 0;
        int aiLevel = 0;

        // --- Calculate ---
        int currentAstroFertility = CalculateCurrentAstroFertility(playerBase, astro);

        int baseEconomy = CalculateBaseEconomy(playerBase, astro);
        int energyProduction = CalculateEnergyProduction(playerBase, astro);
        int energyConsumption = CalculateEnergyConsumption(playerBase);
        int constructionCapacity = CalculateConstructionCapacity(playerBase, astro, cyberneticsLevel);
        int productionCapacity = CalculateProductionCapacity(playerBase, astro, cyberneticsLevel);
        int researchCapacity = CalculateResearchCapacity(playerBase, aiLevel); // Per Base
        int maxPopulation = CalculateMaxPopulation(playerBase, currentAstroFertility);
        int currentPopulationUsed = CalculateCurrentPopulationUsed(playerBase);
        int maxArea = CalculateMaxArea(playerBase, astro);
        int currentAreaUsed = CalculateCurrentAreaUsed(playerBase);

        // --- Return Results ---
        return new CalculatedBaseStats(
            BaseEconomy: baseEconomy,
            EnergyProduction: energyProduction,
            EnergyConsumption: energyConsumption,
            ConstructionCapacity: constructionCapacity,
            ProductionCapacity: productionCapacity,
            ResearchCapacity: researchCapacity,
            MaxPopulation: maxPopulation,
            CurrentPopulationUsed: currentPopulationUsed,
            MaxArea: maxArea,
            CurrentAreaUsed: currentAreaUsed
        );
    }

    #region Private Calculation Helpers

    private int CalculateBaseEconomy(Base playerBase, Astro astro)
    {
        int economy = 0;
        economy += GetStructureLevel(playerBase, StructureType.MetalRefineries) * 1;
        economy += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 1;
        economy += GetStructureLevel(playerBase, StructureType.Shipyards) * 1;
        economy += GetStructureLevel(playerBase, StructureType.CrystalMines) * astro.CrystalsPotential;
        economy += GetStructureLevel(playerBase, StructureType.Spaceports) * 2;
        economy += GetStructureLevel(playerBase, StructureType.EconomicCenters) * 3;
        economy += GetStructureLevel(playerBase, StructureType.CommandCenters) * 1;
        // Ignoring Capital structure for now
        return economy;
    }

    private int CalculateEnergyProduction(Base playerBase, Astro astro)
    {
        int production = 5; // Base production
        production += GetStructureLevel(playerBase, StructureType.SolarPlants) * astro.SolarPotential;
        production += GetStructureLevel(playerBase, StructureType.GasPlants) * astro.GasPotential;
        production += GetStructureLevel(playerBase, StructureType.FusionPlants) * 4;
        production += GetStructureLevel(playerBase, StructureType.AntimatterPlants) * 10;
        production += GetStructureLevel(playerBase, StructureType.OrbitalPlants) * 12;
        return production;
    }

    private int CalculateEnergyConsumption(Base playerBase)
    {
        int consumption = 0;
        if (playerBase.Structures == null) return 0;
        foreach (var structureEntry in playerBase.Structures)
        {
            // Get total E cost for all levels up to current level
            consumption += StructureDataLookup.GetTotalEnergyCost(structureEntry.Key, structureEntry.Value);
        }
        // Note: Defenses also consume energy, need to include if tracked in Base.Structures
        return consumption;
    }

    private int CalculateConstructionCapacity(Base playerBase, Astro astro, int cyberneticsLevel)
    {
        int baseValue = 15;
        int structureBonus = 0;
        structureBonus += GetStructureLevel(playerBase, StructureType.MetalRefineries) * astro.MetalPotential;
        structureBonus += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.NaniteFactories) * 4;
        structureBonus += GetStructureLevel(playerBase, StructureType.AndroidFactories) * 6;

        double multiplier = (1.0 + 0.05 * cyberneticsLevel); // Tech Multiplier
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateProductionCapacity(Base playerBase, Astro astro, int cyberneticsLevel)
    {
        int baseValue = 0;
        int structureBonus = 0;
        // General Production Structures
        structureBonus += GetStructureLevel(playerBase, StructureType.MetalRefineries) * astro.MetalPotential;
        structureBonus += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.NaniteFactories) * 4;
        structureBonus += GetStructureLevel(playerBase, StructureType.AndroidFactories) * 6;
        // Specific Production Structures
        structureBonus += GetStructureLevel(playerBase, StructureType.Shipyards) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.OrbitalShipyards) * 8;

        double multiplier = (1.0 + 0.05 * cyberneticsLevel); // Tech Multiplier
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateResearchCapacity(Base playerBase, int aiLevel) // Per Base
    {
        int baseValue = 0;
        int structureBonus = GetStructureLevel(playerBase, StructureType.ResearchLabs) * 8;

        double multiplier = (1.0 + 0.05 * aiLevel); // Tech Multiplier
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateCurrentAstroFertility(Base playerBase, Astro astro)
    {
        // BaseFertility on Astro should incorporate positional modifiers
        int biosphereBonus = GetStructureLevel(playerBase, StructureType.BiosphereModification); // Bonus is +1 Fert per level
        return astro.BaseFertility + biosphereBonus;
    }

    private int CalculateMaxPopulation(Base playerBase, int currentAstroFertility)
    {
        int maxPop = 0;
        maxPop += GetStructureLevel(playerBase, StructureType.UrbanStructures) * currentAstroFertility;
        maxPop += GetStructureLevel(playerBase, StructureType.OrbitalBase) * 10;
        return maxPop;
    }

    private int CalculateCurrentPopulationUsed(Base playerBase)
    {
        int used = 0;
        if (playerBase.Structures == null) return 0;
        foreach (var structureEntry in playerBase.Structures)
        {
            // Get total P cost for all levels up to current level
            used += StructureDataLookup.GetTotalPopulationCost(structureEntry.Key, structureEntry.Value);
        }
        // Note: Defenses also use population
        return used;
    }

    private int CalculateMaxArea(Base playerBase, Astro astro)
    {
        int maxArea = astro.BaseArea;
        maxArea += GetStructureLevel(playerBase, StructureType.Terraform) * 5;
        maxArea += GetStructureLevel(playerBase, StructureType.MultiLevelPlatforms) * 10;
        return maxArea;
    }

    private int CalculateCurrentAreaUsed(Base playerBase)
    {
        int used = 0;
        if (playerBase.Structures == null) return 0;
        foreach (var structureEntry in playerBase.Structures)
        {
            // Get total A cost for all levels up to current level
            used += StructureDataLookup.GetTotalAreaCost(structureEntry.Key, structureEntry.Value);
        }
        return used;
    }

    // --- General Helper ---
    private int GetStructureLevel(Base playerBase, StructureType type)
    {
        // Safely get level from dictionary, return 0 if not found or if Structures is null
        return playerBase?.Structures?.TryGetValue(type, out int level) ?? false ? level : 0;
    }

    // Placeholder for getting tech levels - For M1, we assume 0
    // private int GetPlayerTechLevel(Guid playerId, TechnologyType techType) => 0;

    #endregion
}

// Placeholder - Should be defined in Core project
public enum TechnologyType { Cybernetics, ArtificialIntelligence /*, ... */ }