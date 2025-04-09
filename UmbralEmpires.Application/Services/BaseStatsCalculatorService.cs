using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using System; // For Math.Floor
using System.Linq; // If needed later for complex lookups

namespace UmbralEmpires.Application.Services;

public class BaseStatsCalculatorService : IBaseStatsCalculatorService
{
    // Dependencies for tech levels or structure cost data can be injected via constructor later

    public CalculatedBaseStats CalculateStats(Base playerBase, Astro astro)
    {
        // --- Prerequisites (Placeholders - need actual data sources later) ---
        // We need the player's relevant tech levels for multipliers
        int cyberneticsLevel = GetPlayerTechLevel(playerBase.PlayerId, TechnologyType.Cybernetics); // Example placeholder
        int aiLevel = GetPlayerTechLevel(playerBase.PlayerId, TechnologyType.ArtificialIntelligence); // Example placeholder

        // We need the Astro's current fertility (considering Biosphere mods)
        int currentAstroFertility = CalculateCurrentAstroFertility(playerBase, astro);

        // --- Calculate Stats using GDD Formulas ---
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
    // --- Implementations based on GDD 4.3 ---

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
        // Capital contribution ignored for now
        return economy;
    }

    private int CalculateEnergyProduction(Base playerBase, Astro astro)
    {
        int production = 5; // Base
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
        foreach (var structureEntry in playerBase.Structures)
        {
            // Sum the energy cost for *all levels* up to the current level
            consumption += StructureDataLookup.GetTotalEnergyCost(structureEntry.Key, structureEntry.Value);
        }
        // Add defense consumption later
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
        double multiplier = (1.0 + 0.05 * cyberneticsLevel);
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
        double multiplier = (1.0 + 0.05 * cyberneticsLevel);
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateResearchCapacity(Base playerBase, int aiLevel) // Per Base
    {
        int baseValue = 0;
        int structureBonus = GetStructureLevel(playerBase, StructureType.ResearchLabs) * 8;
        double multiplier = (1.0 + 0.05 * aiLevel);
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateCurrentAstroFertility(Base playerBase, Astro astro)
    {
        int biosphereBonus = GetStructureLevel(playerBase, StructureType.BiosphereModification);
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
        foreach (var structureEntry in playerBase.Structures)
        {
            // Sum population cost for all levels
            used += StructureDataLookup.GetTotalPopulationCost(structureEntry.Key, structureEntry.Value);
        }
        // Add defense population later
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
        foreach (var structureEntry in playerBase.Structures)
        {
            // Sum area cost for all levels
            used += StructureDataLookup.GetTotalAreaCost(structureEntry.Key, structureEntry.Value);
        }
        return used;
    }

    // --- Helpers ---
    private int GetStructureLevel(Base playerBase, StructureType type)
    {
        return playerBase.Structures.TryGetValue(type, out int level) ? level : 0;
    }

    // Placeholder for getting player's tech level - Requires data access/another service
    private int GetPlayerTechLevel(Guid playerId, TechnologyType techType) => 0; // TODO: Replace with actual implementation

    // Placeholder for accessing static structure data (costs per level)
    // This data needs to be loaded from config or hardcoded structures
    private static class StructureDataLookup
    {
        // These need to return the SUM of costs for levels 1 to 'level'
        public static int GetTotalEnergyCost(StructureType type, int level) => 0; // TODO: Implement lookup
        public static int GetTotalPopulationCost(StructureType type, int level)
        {
            // Example: Assume most cost 1 pop per level (exceptions need handling)
            bool costsPop = type != StructureType.OrbitalShipyards && type != StructureType.Terraform /* ... other exceptions ... */;
            return costsPop ? level : 0;
        }
        public static int GetTotalAreaCost(StructureType type, int level)
        {
            // Example: Assume most cost 1 area per level (exceptions need handling)
            bool costsArea = type != StructureType.OrbitalBase && type != StructureType.OrbitalPlants /* ... other exceptions ... */;
            return costsArea ? level : 0;
        }
    }

    #endregion
}

// Assumed enum - needs defining in Core
public enum TechnologyType { Cybernetics, ArtificialIntelligence /*, ... */ }