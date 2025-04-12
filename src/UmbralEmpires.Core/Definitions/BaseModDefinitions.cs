using System.Collections.Generic;
namespace UmbralEmpires.Core.Definitions;

public record BaseModDefinitions
{
    // Initialize lists to avoid nulls
    public List<StructureDefinition> Structures { get; init; } = new();
    public List<TechnologyDefinition> Technologies { get; init; } = new();
    public List<UnitDefinition> Units { get; init; } = new();
}