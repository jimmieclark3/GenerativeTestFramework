// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Constraints and preferences for test generation.
/// </summary>
public class GenerationConstraints
{
    [JsonPropertyName("maxTestCases")]
    public int MaxTestCases { get; set; }

    [JsonPropertyName("timeBudgetMs")]
    public int TimeBudgetMs { get; set; }

    [JsonPropertyName("forbidIO")]
    public bool ForbidIO { get; set; }

    [JsonPropertyName("deterministicOnly")]
    public bool DeterministicOnly { get; set; }

    [JsonPropertyName("allowedFrameworks")]
    public string[] AllowedFrameworks { get; set; } = Array.Empty<string>(); // xunit, nunit, mstest
}

