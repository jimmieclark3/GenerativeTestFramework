// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents an assembly/module that contains uncovered methods.
/// </summary>
public class ModuleTarget
{
    [JsonPropertyName("assemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("assemblyPath")]
    public string AssemblyPath { get; set; } = string.Empty;

    [JsonPropertyName("methods")]
    public MethodTarget[] Methods { get; set; } = Array.Empty<MethodTarget>();
}

