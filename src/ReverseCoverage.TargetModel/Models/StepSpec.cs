// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents a step in a test case (arrange, act, or assert).
/// </summary>
public class StepSpec
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty; // CreateInstance, CreateMock, SetProperty, CallMethod, AssignVar, etc.

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty; // template-like instruction; not raw C#
}

