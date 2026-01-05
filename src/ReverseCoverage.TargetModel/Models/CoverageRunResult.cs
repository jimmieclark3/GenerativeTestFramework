// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json.Serialization;

namespace ReverseCoverage.TargetModel.Models;

/// <summary>
/// Represents the result of a coverage run execution.
/// </summary>
public class CoverageRunResult
{
    [JsonPropertyName("runId")]
    public string RunId { get; set; } = string.Empty;

    [JsonPropertyName("timestampUtc")]
    public string TimestampUtc { get; set; } = string.Empty;

    [JsonPropertyName("solutionOrProjectPath")]
    public string SolutionOrProjectPath { get; set; } = string.Empty;

    [JsonPropertyName("testProjectPaths")]
    public string[] TestProjectPaths { get; set; } = Array.Empty<string>();

    [JsonPropertyName("coverageXmlPaths")]
    public string[] CoverageXmlPaths { get; set; } = Array.Empty<string>();

    [JsonPropertyName("exitCode")]
    public int ExitCode { get; set; }

    [JsonPropertyName("stdoutPath")]
    public string StdoutPath { get; set; } = string.Empty;

    [JsonPropertyName("stderrPath")]
    public string StderrPath { get; set; } = string.Empty;
}

