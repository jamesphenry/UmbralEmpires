namespace UmbralEmpires.Core.World;

// Represents a celestial body (Planet or Moon) that can potentially host a base.
public class Astro
{
    public Guid Id { get; private set; }
    public AstroCoordinates Coordinates { get; private set; } // Value Object holding Galaxy:Region:System:Orbit
    public TerrainType Terrain { get; private set; }         // Enum for terrain types
    public bool IsPlanet { get; private set; }              // True if planet, False if moon

    // --- Base Potentials (Derived from Terrain & Position Modifiers) ---
    public int MetalPotential { get; private set; }
    public int GasPotential { get; private set; }
    public int CrystalsPotential { get; private set; }
    public int SolarPotential { get; private set; } // Base value for Solar Plants
    public int BaseFertility { get; private set; }  // Base value for Urban Structures (before Biosphere)
    public int BaseArea { get; private set; }       // Base value for available area (before Terraform/Platforms)

    // --- Relationships ---
    // Link to the Base built here, if any. Null if uninhabited.
    public Guid? BaseId { get; private set; }
    // We might add navigation properties later if needed for EF Core relationships,
    // but for now, just the ID is often sufficient in the domain model.

    // --- Constructor ---
    // Potentials are calculated externally (e.g., during world generation) and passed in.
    public Astro(
        Guid id,
        AstroCoordinates coordinates,
        TerrainType terrain,
        bool isPlanet,
        int metalPotential,
        int gasPotential,
        int crystalsPotential,
        int solarPotential,
        int baseFertility,
        int baseArea)
    {
        // Basic validation could be added here (e.g., non-negative potentials)
        Id = id;
        Coordinates = coordinates ?? throw new ArgumentNullException(nameof(coordinates));
        Terrain = terrain;
        IsPlanet = isPlanet;
        MetalPotential = metalPotential;
        GasPotential = gasPotential;
        CrystalsPotential = crystalsPotential;
        SolarPotential = solarPotential;
        BaseFertility = baseFertility;
        BaseArea = baseArea;
        BaseId = null; // Astros start uninhabited
    }

    // --- Methods ---
    public void AssignBase(Guid baseId)
    {
        if (BaseId.HasValue)
        {
            // Maybe throw an exception or handle this case? Astro already has a base.
            // For now, let's just overwrite, assuming prior checks.
        }
        BaseId = baseId;
    }

    public void RemoveBase()
    {
        BaseId = null;
    }

    // Private constructor for EF Core hydration
    private Astro()
    {
        // Required for EF Core to reconstruct the object from the database
        Coordinates = null!; // Initialize non-nullable properties for EF Core if needed
    }
}

// --- Supporting Types (also in Core project) ---

// Using a record for AstroCoordinates makes sense as it's primarily data.
public record AstroCoordinates(string Galaxy, int Region, int System, int Orbit);

// Enum definition based on GDD Section 3.5
public enum TerrainType
{
    // Note: Order might matter if used numerically, otherwise alphabetical is fine.
    Earthly = 0, // Player start default
    Arid = 1,
    Asteroid = 2,
    Craters = 3,
    Crystalline = 4,
    Gaia = 5,
    Glacial = 6,
    Magma = 7,
    Metallic = 8,
    Oceanic = 9,
    Radioactive = 10,
    Rocky = 11,
    Toxic = 12,
    Tundra = 13,
    Volcanic = 14
}