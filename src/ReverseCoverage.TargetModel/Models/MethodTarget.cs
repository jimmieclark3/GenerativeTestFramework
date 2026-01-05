// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents a method that has uncovered code regions.
/// </summary>
public class MethodTarget
{
    [JsonPropertyName("methodId")]
    public string MethodId { get; set; } = string.Empty;

    [JsonPropertyName("typeFullName")]
    public string TypeFullName { get; set; } = string.Empty;

    [JsonPropertyName("methodDisplayName")]
    public string MethodDisplayName { get; set; } = string.Empty;

    [JsonPropertyName("sourceFiles")]
    public string[] SourceFiles { get; set; } = Array.Empty<string>();

    [JsonPropertyName("uncoveredSequencePoints")]
    public SequencePoint[] UncoveredSequencePoints { get; set; } = Array.Empty<SequencePoint>();

    [JsonPropertyName("uncoveredBranchPoints")]
    public BranchPoint[] UncoveredBranchPoints { get; set; } = Array.Empty<BranchPoint>();
}

