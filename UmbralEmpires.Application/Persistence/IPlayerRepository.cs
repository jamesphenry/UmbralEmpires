// --- File: IPlayerRepository.cs ---
using UmbralEmpires.Core.Gameplay;
using System;
using System.Threading.Tasks;

namespace UmbralEmpires.Application.Persistence;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task AddAsync(Player player);
    Task UpdateAsync(Player player);
}