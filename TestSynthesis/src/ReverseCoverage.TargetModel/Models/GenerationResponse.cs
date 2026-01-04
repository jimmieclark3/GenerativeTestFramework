// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Response from the AI plugin containing proposed test specifications.
/// </summary>
public class GenerationResponse
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;

    [JsonPropertyName("proposedTests")]
    public TestCaseSpec[] ProposedTests { get; set; } = Array.Empty<TestCaseSpec>();

    [JsonPropertyName("notes")]
    public string[] Notes { get; set; } = Array.Empty<string>();
}

