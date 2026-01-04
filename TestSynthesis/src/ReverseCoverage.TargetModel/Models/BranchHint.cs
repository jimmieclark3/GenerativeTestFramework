// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Provides hints about branch conditions to help guide test generation.
/// </summary>
public class BranchHint
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty; // NullCheck, Compare, Switch, EnumMatch, ParseTry, LengthCheck, etc.

    [JsonPropertyName("operands")]
    public string[] Operands { get; set; } = Array.Empty<string>(); // parameter/property names

    [JsonPropertyName("suggestedMutations")]
    public string[] SuggestedMutations { get; set; } = Array.Empty<string>(); // human-readable guidance
}

