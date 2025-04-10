// --- File: BaseStatsCalculatorService.cs ---
using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;
using UmbralEmpires.Application.GameData;
using System;
using System.Linq;

namespace UmbralEmpires.Application.Services;

public class BaseStatsCalculatorService : IBaseStatsCalculatorService
{
    // No constructor dependencies needed for this version

    // ***MODIFIED***: Method now accepts tech levels
    public CalculatedBaseStats CalculateStats(Base playerBase, Astro astro, int cyberneticsLevel=0, int aiLevel=0)
    {
        // Tech levels are now passed in, no longer assuming 0 here.

        // --- Calculate Intermediate Values ---
        int currentAstroFertility = CalculateCurrentAstroFertility(playerBase, astro);

        // --- Calculate Final Stats using Helper Methods ---
        int baseEconomy = CalculateBaseEconomy(playerBase, astro);
        int energyProduction = CalculateEnergyProduction(playerBase, astro);
        int energyConsumption = CalculateEnergyConsumption(playerBase);
        // Pass tech levels to capacity calculations
        int constructionCapacity = CalculateConstructionCapacity(playerBase, astro, cyberneticsLevel);
        int productionCapacity = CalculateProductionCapacity(playerBase, astro, cyberneticsLevel);
        int researchCapacity = CalculateResearchCapacity(playerBase, aiLevel); // Pass AI level
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

    // --- Private Calculation Helpers ---
    // Methods CalculateBaseEconomy, CalculateEnergyProduction, CalculateEnergyConsumption,
    // CalculateCurrentAstroFertility, CalculateMaxPopulation, CalculateCurrentPopulationUsed,
    // CalculateMaxArea, CalculateCurrentAreaUsed, GetStructureLevel remain IDENTICAL to before.
    // Only the capacity calculation methods need to use the passed-in parameters.

    private int CalculateConstructionCapacity(Base playerBase, Astro astro, int cyberneticsLevel) // Uses parameter
    {
        int baseValue = 15;
        int structureBonus = 0;
        structureBonus += GetStructureLevel(playerBase, StructureType.MetalRefineries) * astro.MetalPotential;
        structureBonus += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.NaniteFactories) * 4;
        structureBonus += GetStructureLevel(playerBase, StructureType.AndroidFactories) * 6;

        double multiplier = (1.0 + 0.05 * cyberneticsLevel); // Use parameter
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateProductionCapacity(Base playerBase, Astro astro, int cyberneticsLevel) // Uses parameter
    {
        int baseValue = 0;
        int structureBonus = 0;
        // General Production Structures
        structureBonus += GetStructureLevel(playerBase, StructureType.MetalRefineries) * astro.MetalPotential;
        // ... (Robotic, Nanite, Android) ...
        structureBonus += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.NaniteFactories) * 4;
        structureBonus += GetStructureLevel(playerBase, StructureType.AndroidFactories) * 6;
        // Specific Production Structures
        structureBonus += GetStructureLevel(playerBase, StructureType.Shipyards) * 2;
        structureBonus += GetStructureLevel(playerBase, StructureType.OrbitalShipyards) * 8;

        double multiplier = (1.0 + 0.05 * cyberneticsLevel); // Use parameter
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    private int CalculateResearchCapacity(Base playerBase, int aiLevel) // Uses parameter (Per Base)
    {
        int baseValue = 0;
        int structureBonus = GetStructureLevel(playerBase, StructureType.ResearchLabs) * 8;

        double multiplier = (1.0 + 0.05 * aiLevel); // Use parameter
        return (int)Math.Floor((baseValue + structureBonus) * multiplier);
    }

    // Methods CalculateCurrentAstroFertility, CalculateMaxPopulation, CalculateCurrentPopulationUsed,
    // CalculateMaxArea, CalculateCurrentAreaUsed, GetStructureLevel remain IDENTICAL to before.
    #region Calculation Helpers - Identical Code
    private int CalculateCurrentAstroFertility(Base playerBase, Astro astro) { int biosphereBonus = GetStructureLevel(playerBase, StructureType.BiosphereModification); return astro.BaseFertility + biosphereBonus; }
    private int CalculateMaxPopulation(Base playerBase, int currentAstroFertility) { int maxPop = 0; maxPop += GetStructureLevel(playerBase, StructureType.UrbanStructures) * currentAstroFertility; maxPop += GetStructureLevel(playerBase, StructureType.OrbitalBase) * 10; return maxPop; }
    private int CalculateCurrentPopulationUsed(Base playerBase) { int used = 0; if (playerBase.Structures == null) return 0; foreach (var structureEntry in playerBase.Structures) { used += StructureDataLookup.GetTotalPopulationCost(structureEntry.Key, structureEntry.Value); } return used; }
    private int CalculateMaxArea(Base playerBase, Astro astro) { int maxArea = astro.BaseArea; maxArea += GetStructureLevel(playerBase, StructureType.Terraform) * 5; maxArea += GetStructureLevel(playerBase, StructureType.MultiLevelPlatforms) * 10; return maxArea; }
    private int CalculateCurrentAreaUsed(Base playerBase) { int used = 0; if (playerBase.Structures == null) return 0; foreach (var structureEntry in playerBase.Structures) { used += StructureDataLookup.GetTotalAreaCost(structureEntry.Key, structureEntry.Value); } return used; }
    private int GetStructureLevel(Base playerBase, StructureType type) { return playerBase?.Structures?.TryGetValue(type, out int level) ?? false ? level : 0; }
    #endregion

    private int CalculateBaseEconomy(Base playerBase, Astro astro)
    {
        int economy = 0; // Start at base 0
        if (playerBase.Structures == null) return 0;

        // Sum contributions based on playerBase.Structures and GDD/StructureDataLookup
        economy += GetStructureLevel(playerBase, StructureType.MetalRefineries) * 1;
        economy += GetStructureLevel(playerBase, StructureType.RoboticFactories) * 1;
        economy += GetStructureLevel(playerBase, StructureType.Shipyards) * 1;
        economy += GetStructureLevel(playerBase, StructureType.CrystalMines) * astro.CrystalsPotential; // Uses Astro potential
        economy += GetStructureLevel(playerBase, StructureType.Spaceports) * 2;
        economy += GetStructureLevel(playerBase, StructureType.EconomicCenters) * 3;
        economy += GetStructureLevel(playerBase, StructureType.CommandCenters) * 1;
        // Capital structure effect ignored for now (part is base, part empire-wide)
        return economy;
    }

    private int CalculateEnergyProduction(Base playerBase, Astro astro)
    {
        int production = 5; // Base production
        if (playerBase.Structures == null) return production;

        // Sum contributions from power plants based on playerBase.Structures and astro potentials
        production += GetStructureLevel(playerBase, StructureType.SolarPlants) * astro.SolarPotential;
        production += GetStructureLevel(playerBase, StructureType.GasPlants) * astro.GasPotential;
        // Production bonus from structure *level* (e.g., Lvl 2 Fusion gives +8 total production)
        production += GetStructureLevel(playerBase, StructureType.FusionPlants) * 4;
        production += GetStructureLevel(playerBase, StructureType.AntimatterPlants) * 10;
        production += GetStructureLevel(playerBase, StructureType.OrbitalPlants) * 12;
        return production;
    }

    private int CalculateEnergyConsumption(Base playerBase)
    {
        int consumption = 0;
        if (playerBase.Structures == null) return 0; // Safety check

        foreach (var structureEntry in playerBase.Structures)
        {
            // Uses static lookup to get TOTAL E cost for all levels up to current level
            // (Total Req = ReqPerLevel * CurrentLevel)
            consumption += StructureDataLookup.GetTotalEnergyCost(structureEntry.Key, structureEntry.Value);
        }
        // TODO: Add defense consumption later if defenses are tracked in Base.Structures
        return consumption;
    }
}