// tests/UmbralEmpires.Tests/TestDataBuilders/BaseModDefinitionsBuilder.cs
using System.Collections.Generic;
using System.Text.Json;
using UmbralEmpires.Core.Definitions; // Assuming definitions are here

namespace UmbralEmpires.Tests.TestDataBuilders;

public class BaseModDefinitionsBuilder
{
    private readonly List<StructureDefinition> _structures = new();
    private readonly List<TechnologyDefinition> _technologies = new();
    private readonly List<UnitDefinition> _units = new();

    public BaseModDefinitionsBuilder WithUnit(UnitDefinition unit)
    {
        _units.Add(unit);
        return this;
    }

    public BaseModDefinitionsBuilder WithStructure(StructureDefinition structure)
    {
        _structures.Add(structure);
        return this;
    }

    // Added basic WithTechnology
    public BaseModDefinitionsBuilder WithTechnology(TechnologyDefinition technology)
    {
        _technologies.Add(technology);
        return this;
    }

    // ... Add similar 'With...' methods for Units, Defenses etc. ...

    public string BuildJson()
    {
        var definitions = new BaseModDefinitions
        {
            Structures = _structures,
            Technologies = _technologies,
            Units = _units
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(definitions, options);
    }
}