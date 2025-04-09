using Microsoft.EntityFrameworkCore; // Required for ToListAsync, FindAsync, Entry, EntityState
using UmbralEmpires.Application.Persistence; // For IBaseRepository
using UmbralEmpires.Core.Gameplay;         // For Base entity
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UmbralEmpires.Data.Repositories;

public class BaseRepository : IBaseRepository
{
    private readonly UmbralDbContext _context;

    // DbContext is injected via constructor
    public BaseRepository(UmbralDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Base?> GetByIdAsync(Guid id)
    {
        // Use FindAsync for efficient lookup by primary key
        return await _context.Bases.FindAsync(id);
    }

    public async Task<IEnumerable<Base>> GetByPlayerIdAsync(Guid playerId)
    {
        // Use Where clause and execute query asynchronously
        return await _context.Bases
                             .Where(b => b.PlayerId == playerId)
                             .ToListAsync(); // Materialize the results
    }

    public async Task AddAsync(Base playerBase)
    {
        if (playerBase == null) throw new ArgumentNullException(nameof(playerBase));

        // Add the entity to the context's tracking
        await _context.Bases.AddAsync(playerBase);

        // IMPORTANT: SaveChangesAsync is NOT called here.
        // It's typically called once per operation/request at a higher level
        // (e.g., by a Unit of Work pattern or after an application service method completes).
    }

    public Task UpdateAsync(Base playerBase)
    {
        if (playerBase == null) throw new ArgumentNullException(nameof(playerBase));

        // Because EF Core tracks changes on entities loaded from the context,
        // often just modifying the entity properties is enough.
        // Explicitly setting the state ensures EF Core knows it's modified,
        // which can be safer, especially if the entity wasn't tracked before.
        _context.Entry(playerBase).State = EntityState.Modified;

        // SaveChangesAsync is called elsewhere. Return a completed task for the async signature.
        return Task.CompletedTask;
    }
}