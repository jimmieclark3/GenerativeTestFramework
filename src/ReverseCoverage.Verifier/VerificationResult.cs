// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace ReverseCoverage.Verifier;

/// <summary>
/// Result of test verification.
/// </summary>
public class VerificationResult
{
    public bool Accepted { get; set; }
    public CoverageDelta CoverageDelta { get; set; } = new();
    public bool TestsPassed { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsDeterministic { get; set; } = true;
}

/// <summary>
/// Coverage delta information.
/// </summary>
public class CoverageDelta
{
    public double LineDelta { get; set; }
    public double BranchDelta { get; set; }
    public string[] NewProbesHit { get; set; } = Array.Empty<string>();
}

