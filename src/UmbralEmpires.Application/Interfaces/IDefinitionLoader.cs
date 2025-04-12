// src/UmbralEmpires.Application/Interfaces/IDefinitionLoader.cs
using UmbralEmpires.Core.Definitions; // Adjust namespace if StructureDefinition is elsewhere
using System.Collections.Generic;

namespace UmbralEmpires.Application.Interfaces; // Or appropriate namespace

public interface IDefinitionLoader
{
    // Loads all definitions from the main JSON content
    BaseModDefinitions LoadAllDefinitions(string jsonContent);
}