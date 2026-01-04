// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Describes how to handle a dependency when constructing a test harness.
/// </summary>
public class DependencyPlan
{
    [JsonPropertyName("parameterName")]
    public string ParameterName { get; set; } = string.Empty;

    [JsonPropertyName("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = string.Empty; // AutoMock, UseDefaultValue, UseFake, NeedsAdapter

    [JsonPropertyName("notes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Notes { get; set; }
}

