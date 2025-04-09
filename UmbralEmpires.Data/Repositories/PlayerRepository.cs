// --- File: PlayerRepository.cs ---
using Microsoft.EntityFrameworkCore;
using UmbralEmpires.Application.Persistence;
using UmbralEmpires.Core.Gameplay;
using System;
using System.Threading.Tasks;

namespace UmbralEmpires.Data.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly UmbralDbContext _context;

    public PlayerRepository(UmbralDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        // Assumes DbSet<Player> exists on context
        return await _context.Players.FindAsync(id);
    }

    public async Task AddAsync(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        await _context.Players.AddAsync(player);
        // SaveChangesAsync elsewhere
    }

    public Task UpdateAsync(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));
        _context.Entry(player).State = EntityState.Modified;
        // SaveChangesAsync elsewhere
        return Task.CompletedTask;
    }
}