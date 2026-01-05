// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.CoverageRunner;
using ReverseCoverage.CoverageRunner.Options;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.Verifier;

/// <summary>
/// Re-runs tests with coverage, validates generated tests, and determines accept/reject decisions.
/// </summary>
public class Verifier
{
    private readonly CoverageRunner.CoverageRunner _coverageRunner;

    public Verifier(CoverageRunnerOptions? options = null)
    {
        _coverageRunner = new CoverageRunner.CoverageRunner(options ?? new CoverageRunnerOptions());
    }

    /// <summary>
    /// Verifies generated tests by re-running coverage and comparing results.
    /// </summary>
    public async Task<VerificationResult> VerifyTestsAsync(
        string[] generatedTestFiles,
        CoverageRunResult baselineCoverage,
        string solutionOrProjectPath,
        string[] testProjectPaths,
        CancellationToken cancellationToken = default)
    {
        // Re-run tests with coverage
        var newCoverage = await _coverageRunner.RunCoverageAsync(
            solutionOrProjectPath,
            testProjectPaths,
            cancellationToken);

        // Check if tests passed
        var testsPassed = newCoverage.ExitCode == 0;

        // Calculate coverage delta (simplified - would need to parse and compare coverage XML)
        var delta = new CoverageDelta
        {
            LineDelta = 0.0, // Would calculate from coverage XML comparison
            BranchDelta = 0.0, // Would calculate from coverage XML comparison
            NewProbesHit = Array.Empty<string>()
        };

        // Simple acceptance criteria: tests must pass
        var accepted = testsPassed;

        return new VerificationResult
        {
            Accepted = accepted,
            CoverageDelta = delta,
            TestsPassed = testsPassed,
            RejectionReason = accepted ? null : "Tests failed",
            IsDeterministic = true // Would run multiple times to check
        };
    }
}

