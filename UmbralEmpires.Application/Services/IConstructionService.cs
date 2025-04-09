using UmbralEmpires.Core.Gameplay; // For Base, StructureType enum
using System;                     // For Guid
using System.Threading.Tasks;     // For Task

namespace UmbralEmpires.Application.Services;

// Record to return result of queue attempt
public record QueueResult(bool Success, string Message = "");

public interface IConstructionService
{
    /// <summary>
    /// Attempts to add a structure build/upgrade order to a base's construction queue.
    /// Performs validation checks before queueing.
    /// </summary>
    Task<QueueResult> QueueStructureAsync(Guid playerId, Guid baseId, StructureType structureToBuild);
}