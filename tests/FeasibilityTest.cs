// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Diagnostics;
using System.Text.Json;

namespace ReverseCoverage.FeasibilityTests;

/// <summary>
/// End-to-end feasibility test to validate the framework quality.
/// </summary>
public class FeasibilityTest
{
    private readonly string _workspaceRoot;
    private readonly string _artifactsDir;

    public FeasibilityTest()
    {
        _workspaceRoot = GetWorkspaceRoot();
        _artifactsDir = Path.Combine(_workspaceRoot, "artifacts", "feasibility-test");
        Directory.CreateDirectory(_artifactsDir);
    }

    public async Task<FeasibilityResult> RunAsync()
    {
        var result = new FeasibilityResult
        {
            TestStartTime = DateTime.UtcNow,
            WorkspaceRoot = _workspaceRoot
        };

        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("   REVERSE COVERAGE FEASIBILITY TEST");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine();

            // Phase 1: Measure baseline coverage
            Console.WriteLine("PHASE 1: Measuring baseline coverage...");
            var baselineCoverage = await MeasureBaselineCoverageAsync();
            result.BaselineCoverage = baselineCoverage;
            Console.WriteLine($"✓ Baseline: {baselineCoverage.LineCoverage:F1}% line coverage, {baselineCoverage.BranchCoverage:F1}% branch coverage");
            Console.WriteLine();

            // Phase 2: Run orchestrator with mock AI
            Console.WriteLine("PHASE 2: Running orchestrator with test generation...");
            var orchestratorResult = await RunOrchestratorAsync();
            result.OrchestratorExitCode = orchestratorResult.ExitCode;
            result.OrchestratorOutput = orchestratorResult.Output;
            Console.WriteLine($"✓ Orchestrator completed with exit code: {orchestratorResult.ExitCode}");
            Console.WriteLine();

            // Phase 3: Measure improved coverage
            Console.WriteLine("PHASE 3: Measuring improved coverage...");
            var improvedCoverage = await MeasureImprovedCoverageAsync();
            result.ImprovedCoverage = improvedCoverage;
            Console.WriteLine($"✓ Improved: {improvedCoverage.LineCoverage:F1}% line coverage, {improvedCoverage.BranchCoverage:F1}% branch coverage");
            Console.WriteLine();

            // Phase 4: Analyze generated tests
            Console.WriteLine("PHASE 4: Analyzing generated tests...");
            var testAnalysis = await AnalyzeGeneratedTestsAsync();
            result.TestAnalysis = testAnalysis;
            Console.WriteLine($"✓ Generated {testAnalysis.TestCount} tests");
            Console.WriteLine($"✓ {testAnalysis.PassingTests} passing, {testAnalysis.FailingTests} failing");
            Console.WriteLine();

            // Phase 5: Calculate quality metrics
            Console.WriteLine("PHASE 5: Calculating quality metrics...");
            result.CalculateMetrics();
            Console.WriteLine($"✓ Coverage improvement: {result.CoverageImprovement:F1}%");
            Console.WriteLine($"✓ Test quality score: {result.TestQualityScore:F1}%");
            Console.WriteLine();

            result.Success = result.TestQualityScore >= 70.0;
            result.TestEndTime = DateTime.UtcNow;

            // Print final report
            PrintFinalReport(result);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.ToString();
            result.TestEndTime = DateTime.UtcNow;
            Console.WriteLine($"✗ ERROR: {ex.Message}");
            return result;
        }
    }

    private async Task<CoverageMetrics> MeasureBaselineCoverageAsync()
    {
        var testProjectPath = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests", "DemoCalc.Tests.csproj");
        var coverageOutput = Path.Combine(_artifactsDir, "baseline-coverage");
        Directory.CreateDirectory(coverageOutput);

        var result = await RunProcessAsync("dotnet", 
            $"test \"{testProjectPath}\" " +
            $"/p:CollectCoverage=true " +
            $"/p:CoverletOutputFormat=cobertura " +
            $"/p:CoverletOutput=\"{coverageOutput}/\" " +
            $"--verbosity quiet");

        return ParseCoverageReport(Path.Combine(coverageOutput, "coverage.cobertura.xml"));
    }

    private async Task<CoverageMetrics> MeasureImprovedCoverageAsync()
    {
        var testProjectPath = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests", "DemoCalc.Tests.csproj");
        var coverageOutput = Path.Combine(_artifactsDir, "improved-coverage");
        Directory.CreateDirectory(coverageOutput);

        var result = await RunProcessAsync("dotnet", 
            $"test \"{testProjectPath}\" " +
            $"/p:CollectCoverage=true " +
            $"/p:CoverletOutputFormat=cobertura " +
            $"/p:CoverletOutput=\"{coverageOutput}/\" " +
            $"--verbosity quiet");

        return ParseCoverageReport(Path.Combine(coverageOutput, "coverage.cobertura.xml"));
    }

    private async Task<ProcessExecutionResult> RunOrchestratorAsync()
    {
        var orchestratorProject = Path.Combine(_workspaceRoot, "src", "ReverseCoverage.Orchestrator", "ReverseCoverage.Orchestrator.csproj");
        var solutionPath = Path.Combine(_workspaceRoot, "ReverseCoverage.sln");
        var testProjectPath = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests", "DemoCalc.Tests.csproj");

        return await RunProcessAsync("dotnet",
            $"run --project \"{orchestratorProject}\" --no-build -- " +
            $"--solution-path \"{solutionPath}\" " +
            $"--test-project-path \"{testProjectPath}\" " +
            $"--coverage-threshold 80 " +
            $"--iteration-budget 5 " +
            $"--provider LocalLlamaCpp",
            timeout: 120000);
    }

    private async Task<TestAnalysis> AnalyzeGeneratedTestsAsync()
    {
        var testProjectPath = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests", "DemoCalc.Tests.csproj");

        var result = await RunProcessAsync("dotnet",
            $"test \"{testProjectPath}\" --verbosity quiet --logger \"trx;LogFileName=test-results.trx\"");

        // Count tests from output
        var testCount = CountTestsInProject();
        var passingTests = result.ExitCode == 0 ? testCount : 0;
        var failingTests = testCount - passingTests;

        return new TestAnalysis
        {
            TestCount = testCount,
            PassingTests = passingTests,
            FailingTests = failingTests,
            TestFiles = CountTestFiles()
        };
    }

    private CoverageMetrics ParseCoverageReport(string xmlPath)
    {
        var metrics = new CoverageMetrics();

        if (!File.Exists(xmlPath))
        {
            Console.WriteLine($"Warning: Coverage file not found at {xmlPath}");
            return metrics;
        }

        try
        {
            var xml = System.Xml.Linq.XDocument.Load(xmlPath);
            var coverage = xml.Root;

            if (coverage != null)
            {
                var lineRate = coverage.Attribute("line-rate")?.Value;
                var branchRate = coverage.Attribute("branch-rate")?.Value;

                if (double.TryParse(lineRate, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out var lr))
                {
                    metrics.LineCoverage = lr * 100.0;
                }

                if (double.TryParse(branchRate, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var br))
                {
                    metrics.BranchCoverage = br * 100.0;
                }

                metrics.LinesHit = int.Parse(coverage.Attribute("lines-covered")?.Value ?? "0");
                metrics.LinesTotal = int.Parse(coverage.Attribute("lines-valid")?.Value ?? "0");
                metrics.BranchesHit = int.Parse(coverage.Attribute("branches-covered")?.Value ?? "0");
                metrics.BranchesTotal = int.Parse(coverage.Attribute("branches-valid")?.Value ?? "0");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to parse coverage report: {ex.Message}");
        }

        return metrics;
    }

    private int CountTestsInProject()
    {
        var testDir = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests");
        var testFiles = Directory.GetFiles(testDir, "*.cs", SearchOption.AllDirectories);
        
        int count = 0;
        foreach (var file in testFiles)
        {
            var content = File.ReadAllText(file);
            count += System.Text.RegularExpressions.Regex.Matches(content, @"\[Fact\]|\[Theory\]").Count;
        }
        return count;
    }

    private int CountTestFiles()
    {
        var testDir = Path.Combine(_workspaceRoot, "src", "DemoCalc.Tests");
        return Directory.GetFiles(testDir, "*Tests.cs", SearchOption.AllDirectories).Length;
    }

    private async Task<ProcessExecutionResult> RunProcessAsync(string fileName, string arguments, int timeout = 30000)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _workspaceRoot
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception($"Failed to start process: {fileName}");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        var completed = await Task.WhenAny(Task.Delay(timeout), process.WaitForExitAsync());
        
        if (completed != process.WaitForExitAsync())
        {
            process.Kill();
            throw new TimeoutException($"Process timed out after {timeout}ms");
        }

        var output = await outputTask;
        var error = await errorTask;

        return new ProcessExecutionResult
        {
            ExitCode = process.ExitCode,
            Output = output + error
        };
    }

    private void PrintFinalReport(FeasibilityResult result)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   FEASIBILITY TEST RESULTS");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"Test Duration: {result.Duration.TotalSeconds:F1}s");
        Console.WriteLine();
        Console.WriteLine("COVERAGE METRICS:");
        Console.WriteLine($"  Baseline:  {result.BaselineCoverage.LineCoverage:F1}% line, {result.BaselineCoverage.BranchCoverage:F1}% branch");
        Console.WriteLine($"  Improved:  {result.ImprovedCoverage.LineCoverage:F1}% line, {result.ImprovedCoverage.BranchCoverage:F1}% branch");
        Console.WriteLine($"  Δ Change:  +{result.CoverageImprovement:F1}%");
        Console.WriteLine();
        Console.WriteLine("TEST METRICS:");
        Console.WriteLine($"  Tests generated: {result.TestAnalysis.TestCount}");
        Console.WriteLine($"  Tests passing:   {result.TestAnalysis.PassingTests}");
        Console.WriteLine($"  Tests failing:   {result.TestAnalysis.FailingTests}");
        Console.WriteLine($"  Test files:      {result.TestAnalysis.TestFiles}");
        Console.WriteLine();
        Console.WriteLine("QUALITY SCORE:");
        Console.WriteLine($"  Overall: {result.TestQualityScore:F1}%");
        Console.WriteLine();
        Console.WriteLine($"RESULT: {(result.Success ? "✓ PASS" : "✗ FAIL")}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        // Save report to JSON
        var reportPath = Path.Combine(_artifactsDir, "feasibility-report.json");
        File.WriteAllText(reportPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine();
        Console.WriteLine($"Full report saved to: {reportPath}");
    }

    private string GetWorkspaceRoot()
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            if (File.Exists(Path.Combine(current, "ReverseCoverage.sln")))
            {
                return current;
            }
            current = Directory.GetParent(current)?.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}

public class FeasibilityResult
{
    public DateTime TestStartTime { get; set; }
    public DateTime TestEndTime { get; set; }
    public TimeSpan Duration => TestEndTime - TestStartTime;
    public string WorkspaceRoot { get; set; } = "";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public CoverageMetrics BaselineCoverage { get; set; } = new();
    public CoverageMetrics ImprovedCoverage { get; set; } = new();
    public TestAnalysis TestAnalysis { get; set; } = new();
    
    public int OrchestratorExitCode { get; set; }
    public string OrchestratorOutput { get; set; } = "";

    public double CoverageImprovement { get; set; }
    public double TestQualityScore { get; set; }

    public void CalculateMetrics()
    {
        // Coverage improvement (both line and branch)
        var lineDelta = ImprovedCoverage.LineCoverage - BaselineCoverage.LineCoverage;
        var branchDelta = ImprovedCoverage.BranchCoverage - BaselineCoverage.BranchCoverage;
        CoverageImprovement = (lineDelta + branchDelta) / 2.0;

        // Quality score (0-100)
        var coverageScore = Math.Min(100, ImprovedCoverage.LineCoverage);
        var testPassRate = TestAnalysis.TestCount > 0 
            ? (TestAnalysis.PassingTests * 100.0 / TestAnalysis.TestCount) 
            : 0.0;
        var improvementScore = Math.Min(100, CoverageImprovement * 10); // 10% improvement = 100 points

        TestQualityScore = (coverageScore * 0.4) + (testPassRate * 0.4) + (improvementScore * 0.2);
    }
}

public class CoverageMetrics
{
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public int LinesHit { get; set; }
    public int LinesTotal { get; set; }
    public int BranchesHit { get; set; }
    public int BranchesTotal { get; set; }
}

public class TestAnalysis
{
    public int TestCount { get; set; }
    public int PassingTests { get; set; }
    public int FailingTests { get; set; }
    public int TestFiles { get; set; }
}

public class ProcessExecutionResult
{
    public int ExitCode { get; set; }
    public string Output { get; set; } = "";
}
