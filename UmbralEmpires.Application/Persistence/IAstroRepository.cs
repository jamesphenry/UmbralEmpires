// --- File: IAstroRepository.cs ---
using UmbralEmpires.Core.World; // For Astro, AstroCoordinates
using System;                   // For Guid
using System.Threading.Tasks;   // For Task

namespace UmbralEmpires.Application.Persistence;

public interface IAstroRepository
{
    /// <summary>Gets an Astro by its unique ID.</summary>
    Task<Astro?> GetByIdAsync(Guid id);

    /// <summary>Gets an Astro by its coordinates.</summary>
    Task<Astro?> GetByCoordinatesAsync(AstroCoordinates coordinates);

    /// <summary>Adds a new Astro (typically during world generation).</summary>
    Task AddAsync(Astro astro);

    /// <summary>Saves changes to an Astro (e.g., assigning/unassigning BaseId).</summary>
    Task UpdateAsync(Astro astro);

    // We can add other query methods later, like GetByRegionAsync.
}