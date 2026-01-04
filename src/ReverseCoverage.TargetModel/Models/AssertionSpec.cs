// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents an assertion in a test case.
/// </summary>
public class AssertionSpec
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty; // Throws, Equal, NotNull, True, False, Snapshot, etc.

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

