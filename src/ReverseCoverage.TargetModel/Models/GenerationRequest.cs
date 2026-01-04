// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Request sent to the AI plugin to generate test specifications.
/// </summary>
public class GenerationRequest
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("targetMethod")]
    public MethodTarget TargetMethod { get; set; } = new();

    [JsonPropertyName("methodSignature")]
    public string MethodSignature { get; set; } = string.Empty;

    [JsonPropertyName("containingTypeSource")]
    public string ContainingTypeSource { get; set; } = string.Empty;

    [JsonPropertyName("methodSource")]
    public string MethodSource { get; set; } = string.Empty;

    [JsonPropertyName("branchHints")]
    public BranchHint[] BranchHints { get; set; } = Array.Empty<BranchHint>();

    [JsonPropertyName("harnessPlan")]
    public HarnessPlan HarnessPlan { get; set; } = new();

    [JsonPropertyName("constraints")]
    public GenerationConstraints Constraints { get; set; } = new();
}

