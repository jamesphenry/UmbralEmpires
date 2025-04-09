using UmbralEmpires.Core.World; // For StructureType if it lives there, adjust if needed

namespace UmbralEmpires.Core.Gameplay; // Or UmbralEmpires.Core.Domain

public class Base
{
    public Guid Id { get; private set; }
    public Guid AstroId { get; private set; }  // The Astro this base is located on
    public Guid PlayerId { get; private set; } // The owning player
    public string Name { get; private set; }

    // Tracks built structures and their current level (Key: StructureType, Value: Level)
    public Dictionary<StructureType, int> Structures { get; private set; }

    // Tracks items queued for construction
    public List<ConstructionQueueItem> ConstructionQueue { get; private set; }
    // Note: The currently *actively building* item might be tracked separately or inferred

    // --- Constructor for a NEW base ---
    public Base(Guid id, Guid astroId, Guid playerId, string name)
    {
        Id = id;
        AstroId = astroId;
        PlayerId = playerId;
        Name = name; // Default name could be generated too

        // Initialize with starting structures per GDD 4.4
        Structures = new Dictionary<StructureType, int>
        {
            { StructureType.UrbanStructures, 1 }
        };

        ConstructionQueue = new List<ConstructionQueueItem>();
    }

    // --- Methods to Modify State ---
    // These methods primarily update the entity's state.
    // Complex logic (validation, calculation) should ideally live in services.

    public void SetStructureLevel(StructureType type, int level)
    {
        if (level > 0)
        {
            Structures[type] = level;
        }
        else // Level 0 or less means structure is removed
        {
            Structures.Remove(type);
        }
        // Consider raising a Domain Event: StructureLevelChanged(Id, type, level)
    }

    public void AddToConstructionQueue(ConstructionQueueItem item)
    {
        // Basic check, complex validation happens before calling this
        if (ConstructionQueue.Count >= 5) // Use a constant
            throw new InvalidOperationException("Queue is full.");
        ConstructionQueue.Add(item);
        // Consider Domain Event: ItemQueued(Id, item)
    }

    public void RemoveFromConstructionQueue(int index) // Or by item reference/ID
    {
        if (index >= 0 && index < ConstructionQueue.Count)
        {
            var item = ConstructionQueue[index];
            ConstructionQueue.RemoveAt(index);
            // Consider Domain Event: ItemDequeued(Id, item, cancelled: true)
        }
    }

    // Method to potentially signal completion of the first item (simplified)
    public ConstructionQueueItem? CompleteFirstQueueItem()
    {
        if (ConstructionQueue.Count > 0)
        {
            var completedItem = ConstructionQueue[0];
            ConstructionQueue.RemoveAt(0);
            SetStructureLevel(completedItem.StructureType, completedItem.TargetLevel);
            // Consider Domain Event: ItemDequeued(Id, completedItem, cancelled: false)
            return completedItem;
        }
        return null;
    }

    public void Rename(string newName)
    {
        if (!string.IsNullOrWhiteSpace(newName)) // Basic validation
        {
            Name = newName;
            // Consider Domain Event: BaseRenamed(Id, newName)
        }
    }


    // Private constructor for EF Core
    private Base()
    {
        Name = null!;
        Structures = null!;
        ConstructionQueue = null!;
    }
}

// --- Supporting Types ---

// Represents an item in the queue. Includes calculated build time when added.
// Actual remaining time needs to be tracked by the game loop/timing service.
public class ConstructionQueueItem // Changed from record to class
{
    // Properties set when item is created/queued
    public StructureType StructureType { get; init; }
    public int TargetLevel { get; init; }
    public double TotalBuildTimeSeconds { get; init; } // Calculated when added to queue

    // Mutable property to track build progress
    public double RemainingBuildTimeSeconds { get; set; }

    // Constructor used when adding to queue
    public ConstructionQueueItem(StructureType structureType, int targetLevel, double totalBuildTimeSeconds)
    {
        StructureType = structureType;
        TargetLevel = targetLevel;
        TotalBuildTimeSeconds = totalBuildTimeSeconds;
        // Initialize Remaining time - it starts counting down only when active build begins
        RemainingBuildTimeSeconds = totalBuildTimeSeconds;
    }

    // Parameterless constructor potentially needed for EF Core JSON deserialization
    private ConstructionQueueItem()
    {
        // Initialize properties to non-null default if required by compiler/deserializer
        StructureType = default!; // Or a specific default if appropriate
    }
}

// Enum for all structure & defense types (assuming they use the same build queue)
// Needs to be comprehensive based on GDD Tables 6.2 and 6.4
public enum StructureType
{
    // Basic
    UrbanStructures,
    SolarPlants,
    GasPlants,
    ResearchLabs,
    MetalRefineries,
    CrystalMines,
    Shipyards,
    Spaceports,
    CommandCenters,

    // Tier 1 Tech Req
    FusionPlants, // Energy 6
    RoboticFactories, // Computer 2

    // Tier 2 Tech Req
    NaniteFactories, // Computer 10 + Laser 8
    EconomicCenters, // Computer 10
    Terraform, // Computer 10 + Energy 10
    OrbitalBase, // Computer 20
    AntimatterPlants, // Energy 20

    // Tier 3+ Tech Req
    AndroidFactories, // AI 4
    MultiLevelPlatforms, // Armour 22
    JumpGate, // Warp Drive 12 + Energy 20
    BiosphereModification, // Computer 24 + Energy 24
    Capital, // Tachyon Comm 1
    Cybernetics, // AI 6 -> Needed for Orbital Shipyard?
    OrbitalShipyards, // Cybernetics 2
    OrbitalPlants, // Energy 25

    // Defenses
    Barracks, // Laser 1
    LaserTurrets, // Laser 1
    MissileTurrets, // Missiles 1
    PlasmaTurrets, // Plasma 1 + Armour 6
    IonTurrets, // Ion 1 + Armour 10 + Shielding 2
    PhotonTurrets, // Photon 1 + Armour 14 + Shielding 6
    DisruptorTurrets, // Disruptor 1 + Armour 18 + Shielding 8
    DeflectionShields, // Ion 6 + Shielding 10
    PlanetaryShield, // Ion 10 + Shielding 14
    PlanetaryRing // Photon 10 + Armour 22 + Shielding 12
}