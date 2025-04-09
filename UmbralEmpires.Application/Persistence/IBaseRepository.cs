// --- File: IBaseRepository.cs ---
using UmbralEmpires.Core.Gameplay; // For Base
using System;                       // For Guid
using System.Collections.Generic;   // For IEnumerable
using System.Threading.Tasks;       // For Task

namespace UmbralEmpires.Application.Persistence;

public interface IBaseRepository
{
    /// <summary>Gets a Base by its unique ID.</summary>
    Task<Base?> GetByIdAsync(Guid id);

    /// <summary>Gets all Bases owned by a specific player.</summary>
    Task<IEnumerable<Base>> GetByPlayerIdAsync(Guid playerId);

    /// <summary>Adds a new Base to the data store.</summary>
    Task AddAsync(Base playerBase);

    /// <summary>Saves changes made to an existing Base.</summary>
    /// <remarks>EF Core tracks changes, this signals SaveChanges should include this entity.</remarks>
    Task UpdateAsync(Base playerBase);

    // We can add DeleteAsync later if needed for disbanding bases.
}

