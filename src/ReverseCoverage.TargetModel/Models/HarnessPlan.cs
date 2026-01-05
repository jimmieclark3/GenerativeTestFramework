// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Describes how to construct a test harness for a target method.
/// </summary>
public class HarnessPlan
{
    [JsonPropertyName("constructStrategy")]
    public string ConstructStrategy { get; set; } = string.Empty; // Static, PublicCtor, InternalCtor, FactoryMethod, DIContainer, NotCallable

    [JsonPropertyName("ctorOrFactorySignature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CtorOrFactorySignature { get; set; }

    [JsonPropertyName("dependencies")]
    public DependencyPlan[] Dependencies { get; set; } = Array.Empty<DependencyPlan>();
}

