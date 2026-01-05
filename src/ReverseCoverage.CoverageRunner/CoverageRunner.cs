// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Globalization;
using ReverseCoverage.CoverageRunner.Options;
using ReverseCoverage.CoverageRunner.Utilities;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.CoverageRunner;

/// <summary>
/// Executes test runs with coverage instrumentation and collects results.
/// </summary>
public class CoverageRunner
{
    private readonly CoverageRunnerOptions _options;

    public CoverageRunner(CoverageRunnerOptions? options = null)
    {
        _options = options ?? new CoverageRunnerOptions();
    }

    /// <summary>
    /// Runs tests with coverage collection for the specified solution/project and test projects.
    /// </summary>
    public async Task<CoverageRunResult> RunCoverageAsync(
        string solutionOrProjectPath,
        string[] testProjectPaths,
        CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid().ToString("N");
        var timestampUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var artifactsBaseDir = Path.Combine(_options.ArtifactsDirectory, "coverage", runId);
        Directory.CreateDirectory(artifactsBaseDir);

        var coverageOutputDir = Path.Combine(artifactsBaseDir, "coverage");
        Directory.CreateDirectory(coverageOutputDir);

        var arguments = new List<string>
        {
            "test",
            solutionOrProjectPath,
            "/p:CollectCoverage=true",
            $"/p:CoverletOutputFormat={_options.OutputFormat}",
            $"/p:CoverletOutput={coverageOutputDir.Replace('\\', '/')}/",
            "/p:ExcludeByAttribute=\"*.ExcludeFromCodeCoverageAttribute\"",
            "--no-build", // Assume project is already built
            "--verbosity", "normal"
        };

        // Add test project filters if specified
        if (testProjectPaths.Length > 0)
        {
            foreach (var testProject in testProjectPaths)
            {
                arguments.Add(testProject);
            }
        }

        var envVars = new Dictionary<string, string>();
        if (_options.EnableDeterminism)
        {
            envVars["DOTNET_CLI_UI_LANGUAGE"] = "en";
            envVars["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        }

        var stdoutPath = Path.Combine(artifactsBaseDir, "stdout.txt");
        var stderrPath = Path.Combine(artifactsBaseDir, "stderr.txt");

        var result = await ProcessRunner.RunAsync(
            "dotnet",
            string.Join(" ", arguments.Select(a => $"\"{a}\"")),
            workingDirectory: Path.GetDirectoryName(solutionOrProjectPath) ?? Directory.GetCurrentDirectory(),
            environmentVariables: envVars,
            cancellationToken: cancellationToken);

        // Write output to files
        await File.WriteAllTextAsync(stdoutPath, result.Stdout, cancellationToken);
        await File.WriteAllTextAsync(stderrPath, result.Stderr, cancellationToken);

        // Find coverage XML files
        var coverageXmlPaths = new List<string>();
        var coverageFiles = Directory.GetFiles(coverageOutputDir, $"*.{_options.OutputFormat}.xml", SearchOption.AllDirectories);
        coverageXmlPaths.AddRange(coverageFiles);

        return new CoverageRunResult
        {
            RunId = runId,
            TimestampUtc = timestampUtc,
            SolutionOrProjectPath = solutionOrProjectPath,
            TestProjectPaths = testProjectPaths,
            CoverageXmlPaths = coverageXmlPaths.ToArray(),
            ExitCode = result.ExitCode,
            StdoutPath = stdoutPath,
            StderrPath = stderrPath
        };
    }
}

