// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReverseCoverage.FeasibilityTests;

/// <summary>
/// Multi-project feasibility test runner.
/// Tests the framework against multiple codebases in parallel.
/// </summary>
public class MultiProjectFeasibilityTest
{
    private readonly MultiProjectConfig _config;
    private readonly string _workspaceRoot;
    private readonly string _outputDir;

    public MultiProjectFeasibilityTest(string configPath)
    {
        var configJson = File.ReadAllText(configPath);
        _config = JsonSerializer.Deserialize<MultiProjectConfig>(configJson) 
            ?? throw new Exception("Failed to parse config");
        
        _workspaceRoot = GetWorkspaceRoot();
        _outputDir = Path.Combine(_workspaceRoot, _config.OutputDirectory);
        Directory.CreateDirectory(_outputDir);
    }

    public async Task<MultiProjectResult> RunAsync()
    {
        var result = new MultiProjectResult
        {
            StartTime = DateTime.UtcNow,
            ConfigFile = _config
        };

        try
        {
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("   MULTI-PROJECT FEASIBILITY TEST");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine();
            
            var enabledProjects = _config.Projects.Where(p => p.Enabled).ToList();
            Console.WriteLine($"Testing {enabledProjects.Count} project(s):");
            foreach (var proj in enabledProjects)
            {
                Console.WriteLine($"  • {proj.Name} - {proj.Description}");
            }
            Console.WriteLine();

            // Clone external projects if needed
            await CloneExternalProjectsAsync(enabledProjects);

            // Run tests based on execution mode
            if (_config.ExecutionMode == "parallel")
            {
                result.ProjectResults = await RunProjectsInParallelAsync(enabledProjects);
            }
            else
            {
                result.ProjectResults = await RunProjectsSequentiallyAsync(enabledProjects);
            }

            result.EndTime = DateTime.UtcNow;
            result.Success = result.ProjectResults.All(r => r.Success);

            // Generate comparative report
            if (_config.GenerateComparativeReport)
            {
                GenerateComparativeReport(result);
            }

            PrintSummary(result);
            SaveResults(result);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.ToString();
            result.EndTime = DateTime.UtcNow;
            Console.WriteLine($"✗ ERROR: {ex.Message}");
            return result;
        }
    }

    private async Task CloneExternalProjectsAsync(List<ProjectConfig> projects)
    {
        var projectsToClone = projects.Where(p => !string.IsNullOrEmpty(p.GitRepo)).ToList();
        
        if (!projectsToClone.Any())
        {
            return;
        }

        Console.WriteLine("──────────────────────────────────────────────────────");
        Console.WriteLine("Cloning external projects...");
        Console.WriteLine("──────────────────────────────────────────────────────");

        foreach (var project in projectsToClone)
        {
            var clonePath = Path.Combine(_workspaceRoot, project.ClonePath);
            
            if (Directory.Exists(clonePath))
            {
                Console.WriteLine($"  ✓ {project.Name} already cloned");
                continue;
            }

            Console.WriteLine($"  → Cloning {project.Name}...");
            
            try
            {
                var result = await RunProcessAsync("git", 
                    $"clone --depth 1 {project.GitRepo} \"{clonePath}\"",
                    timeout: 180000);
                
                if (result.ExitCode == 0)
                {
                    Console.WriteLine($"  ✓ {project.Name} cloned successfully");
                }
                else
                {
                    Console.WriteLine($"  ✗ Failed to clone {project.Name}");
                    project.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ Error cloning {project.Name}: {ex.Message}");
                project.Enabled = false;
            }
        }
        Console.WriteLine();
    }

    private async Task<List<ProjectTestResult>> RunProjectsSequentiallyAsync(List<ProjectConfig> projects)
    {
        var results = new List<ProjectTestResult>();

        foreach (var project in projects)
        {
            if (_config.StopOnFirstFailure && results.Any(r => !r.Success))
            {
                Console.WriteLine($"⊗ Skipping {project.Name} (stop on first failure)");
                continue;
            }

            var result = await RunSingleProjectAsync(project);
            results.Add(result);
        }

        return results;
    }

    private async Task<List<ProjectTestResult>> RunProjectsInParallelAsync(List<ProjectConfig> projects)
    {
        var results = new ConcurrentBag<ProjectTestResult>();
        var semaphore = new SemaphoreSlim(_config.ParallelismDegree);

        var tasks = projects.Select(async project =>
        {
            await semaphore.WaitAsync();
            try
            {
                var result = await RunSingleProjectAsync(project);
                results.Add(result);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results.OrderBy(r => r.ProjectName).ToList();
    }

    private async Task<ProjectTestResult> RunSingleProjectAsync(ProjectConfig project)
    {
        var result = new ProjectTestResult
        {
            ProjectName = project.Name,
            StartTime = DateTime.UtcNow
        };

        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"   Testing: {project.Name}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        try
        {
            var solutionPath = Path.Combine(_workspaceRoot, project.SolutionPath);
            var testProjectPaths = project.TestProjectPaths
                .Select(p => Path.Combine(_workspaceRoot, p))
                .ToArray();

            // Step 1: Build
            Console.WriteLine($"[{project.Name}] Building...");
            var buildResult = await RunProcessAsync("dotnet", 
                $"build \"{solutionPath}\" --configuration Release --verbosity quiet",
                timeout: 180000);
            
            if (buildResult.ExitCode != 0)
            {
                result.Success = false;
                result.ErrorMessage = "Build failed";
                result.BuildOutput = buildResult.Output;
                Console.WriteLine($"  ✗ Build failed");
                return result;
            }
            Console.WriteLine($"  ✓ Build successful");

            // Step 2: Baseline coverage
            Console.WriteLine($"[{project.Name}] Measuring baseline coverage...");
            result.BaselineCoverage = await MeasureCoverageAsync(project, "baseline");
            Console.WriteLine($"  ✓ Baseline: {result.BaselineCoverage.LineCoverage:F1}% line coverage");

            // Step 3: Run orchestrator
            Console.WriteLine($"[{project.Name}] Running test generation...");
            var orchestratorResult = await RunOrchestratorForProjectAsync(project);
            result.OrchestratorExitCode = orchestratorResult.ExitCode;
            result.OrchestratorOutput = orchestratorResult.Output;
            
            if (orchestratorResult.ExitCode != 0)
            {
                Console.WriteLine($"  ⚠ Orchestrator exited with code {orchestratorResult.ExitCode}");
            }

            // Step 4: Improved coverage
            Console.WriteLine($"[{project.Name}] Measuring improved coverage...");
            result.ImprovedCoverage = await MeasureCoverageAsync(project, "improved");
            Console.WriteLine($"  ✓ Improved: {result.ImprovedCoverage.LineCoverage:F1}% line coverage");

            // Step 5: Calculate metrics
            result.CalculateMetrics();
            result.Success = result.CoverageImprovement >= 0;
            result.EndTime = DateTime.UtcNow;

            Console.WriteLine();
            Console.WriteLine($"[{project.Name}] Results:");
            Console.WriteLine($"  Coverage improvement: +{result.CoverageImprovement:F1}%");
            Console.WriteLine($"  Quality score: {result.QualityScore:F1}%");
            Console.WriteLine($"  Duration: {result.Duration.TotalSeconds:F1}s");
            Console.WriteLine();

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            Console.WriteLine($"  ✗ Error: {ex.Message}");
            Console.WriteLine();
            return result;
        }
    }

    private async Task<CoverageMetrics> MeasureCoverageAsync(ProjectConfig project, string tag)
    {
        var outputDir = Path.Combine(_outputDir, project.Name, tag);
        Directory.CreateDirectory(outputDir);

        var testProject = Path.Combine(_workspaceRoot, project.TestProjectPaths[0]);

        await RunProcessAsync("dotnet",
            $"test \"{testProject}\" " +
            $"/p:CollectCoverage=true " +
            $"/p:CoverletOutputFormat=cobertura " +
            $"/p:CoverletOutput=\"{outputDir}/\" " +
            $"--verbosity quiet --nologo",
            timeout: 120000);

        var coverageFile = Path.Combine(outputDir, "coverage.cobertura.xml");
        return ParseCoverageReport(coverageFile);
    }

    private async Task<ProcessExecutionResult> RunOrchestratorForProjectAsync(ProjectConfig project)
    {
        var orchestratorProject = Path.Combine(_workspaceRoot, 
            "src/ReverseCoverage.Orchestrator/ReverseCoverage.Orchestrator.csproj");
        var solutionPath = Path.Combine(_workspaceRoot, project.SolutionPath);
        var testProjectPath = Path.Combine(_workspaceRoot, project.TestProjectPaths[0]);

        return await RunProcessAsync("dotnet",
            $"run --project \"{orchestratorProject}\" --no-build -- " +
            $"--solution-path \"{solutionPath}\" " +
            $"--test-project-path \"{testProjectPath}\" " +
            $"--coverage-threshold {project.CoverageThreshold} " +
            $"--iteration-budget {project.IterationBudget} " +
            $"--provider Ollama",
            timeout: 300000);
    }

    private CoverageMetrics ParseCoverageReport(string xmlPath)
    {
        var metrics = new CoverageMetrics();

        if (!File.Exists(xmlPath))
        {
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
            }
        }
        catch { }

        return metrics;
    }

    private void GenerateComparativeReport(MultiProjectResult result)
    {
        var reportPath = Path.Combine(_outputDir, "comparative-report.html");
        var html = GenerateHtmlReport(result);
        File.WriteAllText(reportPath, html);
        Console.WriteLine($"Comparative report: {reportPath}");
    }

    private string GenerateHtmlReport(MultiProjectResult result)
    {
        var rows = string.Join("\n", result.ProjectResults.Select(r => $@"
            <tr class='{(r.Success ? "success" : "failure")}'>
                <td>{r.ProjectName}</td>
                <td>{r.BaselineCoverage.LineCoverage:F1}%</td>
                <td>{r.ImprovedCoverage.LineCoverage:F1}%</td>
                <td>{r.CoverageImprovement:F1}%</td>
                <td>{r.QualityScore:F1}%</td>
                <td>{r.Duration.TotalSeconds:F1}s</td>
                <td>{(r.Success ? "✓ Pass" : "✗ Fail")}</td>
            </tr>"));

        return $@"<!DOCTYPE html>
<html>
<head>
    <title>Multi-Project Feasibility Test Results</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; background: #f5f5f5; }}
        h1 {{ color: #333; }}
        .summary {{ background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        table {{ width: 100%; border-collapse: collapse; background: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        th, td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background: #4CAF50; color: white; }}
        tr.success {{ background: #f0f8ff; }}
        tr.failure {{ background: #fff0f0; }}
        .metric {{ display: inline-block; margin: 10px 20px 10px 0; }}
        .metric-value {{ font-size: 24px; font-weight: bold; color: #4CAF50; }}
    </style>
</head>
<body>
    <h1>Multi-Project Feasibility Test Results</h1>
    <div class='summary'>
        <h2>Summary</h2>
        <div class='metric'>
            <div>Projects Tested</div>
            <div class='metric-value'>{result.ProjectResults.Count}</div>
        </div>
        <div class='metric'>
            <div>Successful</div>
            <div class='metric-value'>{result.ProjectResults.Count(r => r.Success)}</div>
        </div>
        <div class='metric'>
            <div>Average Improvement</div>
            <div class='metric-value'>{result.ProjectResults.Average(r => r.CoverageImprovement):F1}%</div>
        </div>
        <div class='metric'>
            <div>Total Duration</div>
            <div class='metric-value'>{result.Duration.TotalMinutes:F1}m</div>
        </div>
    </div>
    <table>
        <tr>
            <th>Project</th>
            <th>Baseline Coverage</th>
            <th>Improved Coverage</th>
            <th>Improvement</th>
            <th>Quality Score</th>
            <th>Duration</th>
            <th>Status</th>
        </tr>
        {rows}
    </table>
</body>
</html>";
    }

    private void PrintSummary(MultiProjectResult result)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("   MULTI-PROJECT TEST SUMMARY");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"Total Projects:  {result.ProjectResults.Count}");
        Console.WriteLine($"Successful:      {result.ProjectResults.Count(r => r.Success)}");
        Console.WriteLine($"Failed:          {result.ProjectResults.Count(r => !r.Success)}");
        Console.WriteLine($"Total Duration:  {result.Duration.TotalMinutes:F1} minutes");
        Console.WriteLine();
        Console.WriteLine("Results by Project:");
        foreach (var proj in result.ProjectResults)
        {
            var status = proj.Success ? "✓" : "✗";
            Console.WriteLine($"  {status} {proj.ProjectName,-20} | Improvement: +{proj.CoverageImprovement:F1}% | Score: {proj.QualityScore:F1}%");
        }
        Console.WriteLine();
        Console.WriteLine($"Overall: {(result.Success ? "✓ PASS" : "✗ FAIL")}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    private void SaveResults(MultiProjectResult result)
    {
        var jsonPath = Path.Combine(_outputDir, "multi-project-results.json");
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
        Console.WriteLine($"\nDetailed results: {jsonPath}");
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

// Configuration classes
public class MultiProjectConfig
{
    [JsonPropertyName("projects")]
    public List<ProjectConfig> Projects { get; set; } = new();

    [JsonPropertyName("executionMode")]
    public string ExecutionMode { get; set; } = "sequential"; // or "parallel"

    [JsonPropertyName("parallelismDegree")]
    public int ParallelismDegree { get; set; } = 2;

    [JsonPropertyName("outputDirectory")]
    public string OutputDirectory { get; set; } = "artifacts/multi-project";

    [JsonPropertyName("generateComparativeReport")]
    public bool GenerateComparativeReport { get; set; } = true;

    [JsonPropertyName("stopOnFirstFailure")]
    public bool StopOnFirstFailure { get; set; } = false;
}

public class ProjectConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("gitRepo")]
    public string GitRepo { get; set; } = "";

    [JsonPropertyName("clonePath")]
    public string ClonePath { get; set; } = "";

    [JsonPropertyName("solutionPath")]
    public string SolutionPath { get; set; } = "";

    [JsonPropertyName("testProjectPaths")]
    public List<string> TestProjectPaths { get; set; } = new();

    [JsonPropertyName("coverageThreshold")]
    public int CoverageThreshold { get; set; } = 80;

    [JsonPropertyName("iterationBudget")]
    public int IterationBudget { get; set; } = 10;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = "";
}

// Result classes
public class MultiProjectResult
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public MultiProjectConfig ConfigFile { get; set; } = new();
    public List<ProjectTestResult> ProjectResults { get; set; } = new();
}

public class ProjectTestResult
{
    public string ProjectName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? BuildOutput { get; set; }
    public CoverageMetrics BaselineCoverage { get; set; } = new();
    public CoverageMetrics ImprovedCoverage { get; set; } = new();
    public int OrchestratorExitCode { get; set; }
    public string OrchestratorOutput { get; set; } = "";
    public double CoverageImprovement { get; set; }
    public double QualityScore { get; set; }

    public void CalculateMetrics()
    {
        var lineDelta = ImprovedCoverage.LineCoverage - BaselineCoverage.LineCoverage;
        var branchDelta = ImprovedCoverage.BranchCoverage - BaselineCoverage.BranchCoverage;
        CoverageImprovement = (lineDelta + branchDelta) / 2.0;

        var coverageScore = Math.Min(100, ImprovedCoverage.LineCoverage);
        var improvementScore = Math.Min(100, CoverageImprovement * 10);
        QualityScore = (coverageScore * 0.6) + (improvementScore * 0.4);
    }
}
