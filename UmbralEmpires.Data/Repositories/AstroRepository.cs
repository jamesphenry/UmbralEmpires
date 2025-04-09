using Microsoft.EntityFrameworkCore;
using UmbralEmpires.Application.Persistence; // For IAstroRepository
using UmbralEmpires.Core.World;            // For Astro, AstroCoordinates
using System;
using System.Threading.Tasks;

namespace UmbralEmpires.Data.Repositories;

public class AstroRepository : IAstroRepository
{
    private readonly UmbralDbContext _context;

    public AstroRepository(UmbralDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Astro?> GetByIdAsync(Guid id)
    {
        return await _context.Astros.FindAsync(id);
    }

    public async Task<Astro?> GetByCoordinatesAsync(AstroCoordinates coordinates)
    {
        // Query based on the properties of the owned AstroCoordinates type
        return await _context.Astros
                             .FirstOrDefaultAsync(a => a.Coordinates.Galaxy == coordinates.Galaxy &&
                                                       a.Coordinates.Region == coordinates.Region &&
                                                       a.Coordinates.System == coordinates.System &&
                                                       a.Coordinates.Orbit == coordinates.Orbit);
    }

    public async Task AddAsync(Astro astro)
    {
        if (astro == null) throw new ArgumentNullException(nameof(astro));
        await _context.Astros.AddAsync(astro);
        // SaveChangesAsync elsewhere
    }

    public Task UpdateAsync(Astro astro)
    {
        if (astro == null) throw new ArgumentNullException(nameof(astro));
        _context.Entry(astro).State = EntityState.Modified;
        // SaveChangesAsync elsewhere
        return Task.CompletedTask;
    }
}