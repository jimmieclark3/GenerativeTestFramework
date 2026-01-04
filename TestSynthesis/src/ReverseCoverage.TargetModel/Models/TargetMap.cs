// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents the normalized map of all uncovered targets across all modules.
/// </summary>
public class TargetMap
{
    [JsonPropertyName("sourceCommit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceCommit { get; set; }

    [JsonPropertyName("generatedAtUtc")]
    public string GeneratedAtUtc { get; set; } = string.Empty;

    [JsonPropertyName("modules")]
    public ModuleTarget[] Modules { get; set; } = Array.Empty<ModuleTarget>();
}

