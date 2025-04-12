// src/UmbralEmpires.Core/Definitions/TechnologyDefinition.cs
using System.Collections.Generic;

namespace UmbralEmpires.Core.Definitions;

public record TechnologyDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int CreditsCost { get; init; }
    public int RequiredLabsLevel { get; init; }
    public List<TechRequirement> RequiresPrerequisites { get; init; } = new();
    public string Description { get; init; } = string.Empty;
}