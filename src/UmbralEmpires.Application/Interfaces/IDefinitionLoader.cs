// src/UmbralEmpires.Application/Interfaces/IDefinitionLoader.cs
using UmbralEmpires.Core.Definitions; // Adjust namespace if StructureDefinition is elsewhere
using System.Collections.Generic;

namespace UmbralEmpires.Application.Interfaces; // Or appropriate namespace

public interface IDefinitionLoader
{
    // Minimal interface for the first test
    IEnumerable<StructureDefinition> LoadStructures(string jsonContent);
}