// tests/UmbralEmpires.Tests/TestDataBuilders/BaseModDefinitionsBuilder.cs
using System.Collections.Generic;
using System.Text.Json; // Add this using
using UmbralEmpires.Core.Definitions;

namespace UmbralEmpires.Tests.TestDataBuilders;

public class BaseModDefinitionsBuilder
{
    private readonly List<StructureDefinition> _structures = new();
    private readonly List<TechnologyDefinition> _technologies = new();
    // Add lists for Units, Defenses etc. as needed

    // 'With...' methods remain the same, returning 'this'
    public BaseModDefinitionsBuilder WithStructure(
        string id,
        string name /*... other params ...*/)
    {
        _structures.Add(new StructureDefinition { Id = id, Name = name /*... other props ...*/ });
        return this;
    }

    public BaseModDefinitionsBuilder WithStructure(StructureDefinition structure)
    {
        _structures.Add(structure);
        return this;
    }

    public BaseModDefinitionsBuilder WithTechnology(string id, string name /*... other params ...*/)
    {
        // _technologies.Add(new TechnologyDefinition { Id = id, Name = name, ... });
        return this;
    }

    // ... Other 'With...' methods ...

    // REMOVE the old Build() method:
    // public BaseModDefinitions Build() { ... }

    // ADD New method to build the object AND serialize it
    public string BuildJson()
    {
        // Construct the final object internally
        var definitions = new BaseModDefinitions
        {
            Structures = _structures,
            Technologies = _technologies
            // Assign other lists...
        };

        // Configure serialization options
        var options = new JsonSerializerOptions
        {
            WriteIndented = true // Keep indentation for readability if desired
        };

        // Serialize and return the JSON string
        return JsonSerializer.Serialize(definitions, options);
    }
}