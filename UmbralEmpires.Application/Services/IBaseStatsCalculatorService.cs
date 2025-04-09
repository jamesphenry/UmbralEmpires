using UmbralEmpires.Core.Gameplay;
using UmbralEmpires.Core.World;

namespace UmbralEmpires.Application.Services; // Or Interfaces

// Defines the calculated stats returned for a Base
public record CalculatedBaseStats(
    int BaseEconomy,
    int EnergyProduction,
    int EnergyConsumption,
    int ConstructionCapacity,
    int ProductionCapacity,
    int ResearchCapacity, // Per-base result
    int MaxPopulation,
    int CurrentPopulationUsed,
    int MaxArea,
    int CurrentAreaUsed
);

// Service definition
public interface IBaseStatsCalculatorService
{
    // Takes the Base state and its Astro's properties
    // May need access to Player's researched tech levels as well
    CalculatedBaseStats CalculateStats(Base playerBase, Astro astro /*, PlayerTechLevels techLevels? */);
}