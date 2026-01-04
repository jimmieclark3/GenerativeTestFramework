// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace ReverseCoverage.CoverageRunner.Options;

/// <summary>
/// Options for configuring coverage runs.
/// </summary>
public class CoverageRunnerOptions
{
    /// <summary>
    /// Output format for coverage data (cobertura or opencover).
    /// </summary>
    public string OutputFormat { get; set; } = "cobertura";

    /// <summary>
    /// Base directory for artifacts.
    /// </summary>
    public string ArtifactsDirectory { get; set; } = "artifacts";

    /// <summary>
    /// Whether to enable deterministic builds.
    /// </summary>
    public bool EnableDeterminism { get; set; } = true;
}

