// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.CommandLine;
using ReverseCoverage.AiTestSynthesis;
using ReverseCoverage.ContextCollector;
using ReverseCoverage.CoverageParser;
using ReverseCoverage.CoverageRunner;
using ReverseCoverage.CoverageRunner.Options;
using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;
using ReverseCoverage.TestEmitter;
using ReverseCoverage.Verifier;

namespace ReverseCoverage.Orchestrator;

/// <summary>
/// Main orchestrator application.
/// </summary>
public static class OrchestratorApp
{
    public static async Task<int> RunAsync(string[] args)
    {
        var solutionPathOption = new Option<string>(
            "--solution-path",
            "Path to solution or project file")
        {
            IsRequired = true
        };

        var testProjectPathOption = new Option<string[]>(
            "--test-project-path",
            "Path(s) to test project(s)")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        var coverageThresholdOption = new Option<int>(
            "--coverage-threshold",
            () => 100,
            "Coverage threshold percentage");

        var iterationBudgetOption = new Option<int>(
            "--iteration-budget",
            () => 100,
            "Maximum number of iterations");

        var providerOption = new Option<string>(
            "--provider",
            () => "LocalLlamaCpp",
            "AI provider: LocalLlamaCpp, OpenAIResponses, CustomHttp, Mock, Claude, Ollama, or ZooLLM");

        var generateAllOption = new Option<bool>(
            "--generate-all",
            () => false,
            "Generate tests for ALL methods (ignore coverage, treat as 0%)");

        var outputFolderOption = new Option<string>(
            "--output-folder",
            "Folder path to write generated tests (default: Generated folder in test project)");

        var zooLLMProviderOption = new Option<string>(
            "--zoollm-provider",
            "ZooLLM provider name (required when using ZooLLM provider)");

        var rootCommand = new RootCommand("Reverse Coverage Test Synthesis Orchestrator")
        {
            solutionPathOption,
            testProjectPathOption,
            coverageThresholdOption,
            iterationBudgetOption,
            providerOption,
            generateAllOption,
            outputFolderOption,
            zooLLMProviderOption
        };

        rootCommand.SetHandler(async (solutionPath, testProjectPaths, coverageThreshold, iterationBudget, provider, generateAll, outputFolder, zooLLMProvider) =>
        {
            await RunOrchestratorAsync(solutionPath, testProjectPaths, coverageThreshold, iterationBudget, provider, generateAll, outputFolder, zooLLMProvider);
        },
        solutionPathOption, testProjectPathOption, coverageThresholdOption, iterationBudgetOption, providerOption, generateAllOption, outputFolderOption, zooLLMProviderOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunOrchestratorAsync(
        string solutionPath,
        string[] testProjectPaths,
        int coverageThreshold,
        int iterationBudget,
        string provider,
        bool generateAll,
        string? outputFolder,
        string? zooLLMProvider)
    {
        Console.WriteLine("Reverse Coverage Test Synthesis Orchestrator");
        Console.WriteLine($"Solution: {solutionPath}");
        Console.WriteLine($"Test Projects: {string.Join(", ", testProjectPaths)}");
        Console.WriteLine($"Coverage Threshold: {coverageThreshold}%");
        Console.WriteLine($"Iteration Budget: {iterationBudget}");
        Console.WriteLine($"Provider: {provider}");
        Console.WriteLine($"Generate All: {generateAll} (treating as 0% coverage)");
        if (!string.IsNullOrEmpty(outputFolder))
        {
            Console.WriteLine($"Output Folder: {outputFolder}");
        }
        Console.WriteLine();

        // Initialize components
        var coverageRunner = new CoverageRunner.CoverageRunner(new CoverageRunnerOptions());
        var coverageParser = new CoverageParser.CoverageParser();
        var contextCollector = new ContextCollector.ContextCollector();
        var providerEnum = Enum.Parse<AiProvider>(provider, ignoreCase: true);
        var aiOptions = new AiTestSynthesisOptions
        {
            Provider = providerEnum
        };
        
        // Configure Ollama-specific settings
        if (providerEnum == AiProvider.Ollama)
        {
            aiOptions.BaseUrl = "http://localhost:11434";
            aiOptions.Model = "gpt-oss:20b"; // Use gpt-oss as specified
            aiOptions.Temperature = 0.0;
            aiOptions.MaxOutputTokens = 2000; // Full output with GPU acceleration
            aiOptions.RequestTimeout = TimeSpan.FromSeconds(60); // 60 seconds should be plenty with GPU
            aiOptions.LogRequestBodies = true;
            aiOptions.LogResponses = true;
        }
        
        // Configure ZooLLM-specific settings
        if (providerEnum == AiProvider.ZooLLM)
        {
            aiOptions.BaseUrl = "https://api.zoollm.com";
            aiOptions.Model = zooLLMProvider; // Provider name from command line
            aiOptions.Temperature = 0.0;
            aiOptions.MaxOutputTokens = 2000;
            aiOptions.RequestTimeout = TimeSpan.FromSeconds(60);
            aiOptions.LogRequestBodies = true;
            aiOptions.LogResponses = true;
        }
        
        var aiClient = new AiTestSynthesisClient(aiOptions);
        var testEmitter = new TestEmitter.TestEmitter();
        var verifier = new Verifier.Verifier();

        // Load solution for context collection first
        await contextCollector.LoadSolutionAsync(solutionPath);

        TargetMap targetMap;
        CoverageRunResult? initialCoverage = null;
        
        if (generateAll)
        {
            // Generate all mode: Find ALL methods from source code, ignore coverage
            Console.WriteLine("Generate-All mode: Finding ALL methods from source code (ignoring coverage)...");
            
            // Extract project name from test project path to filter source projects
            var testProjectName = Path.GetFileNameWithoutExtension(testProjectPaths[0]);
            var sourceProjectName = testProjectName.Replace(".Tests", "").Replace("Tests", "");
            
            Console.WriteLine($"Looking for methods in project matching: '{sourceProjectName}'");
            Console.WriteLine($"Test project name: '{testProjectName}'");
            
            // Try without filter first to see all projects
            Console.WriteLine("Scanning all projects for methods...");
            var allMethods = await contextCollector.FindAllMethodsAsync(null);
            
            Console.WriteLine($"Found {allMethods.Count} total methods across all projects");
            
            // Filter to source project if we found methods
            if (allMethods.Count > 0 && !string.IsNullOrEmpty(sourceProjectName))
            {
                var filtered = allMethods.Where(m => 
                    m.TypeFullName.Contains(sourceProjectName, StringComparison.OrdinalIgnoreCase) ||
                    m.SourceFiles.Any(f => f.Contains(sourceProjectName, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                if (filtered.Count > 0)
                {
                    Console.WriteLine($"Filtered to {filtered.Count} methods in '{sourceProjectName}' project");
                    allMethods = filtered;
                }
            }
            
            Console.WriteLine($"Found {allMethods.Count} methods to generate tests for");
            
            // Convert to TargetMap format
            var modules = allMethods
                .GroupBy(m => m.TypeFullName.Split('.').FirstOrDefault() ?? "Unknown")
                .Select(g => new ReverseCoverage.TargetModel.Models.ModuleTarget
                {
                    AssemblyName = g.Key,
                    AssemblyPath = g.Key,
                    Methods = g.ToArray()
                })
                .ToArray();
            
            targetMap = new ReverseCoverage.TargetModel.Models.TargetMap
            {
                SourceCommit = null,
                GeneratedAtUtc = DateTime.UtcNow.ToString("O"),
                Modules = modules
            };
        }
        else
        {
            // Normal mode: Use coverage to find uncovered methods
            Console.WriteLine("Running initial coverage...");
            initialCoverage = await coverageRunner.RunCoverageAsync(solutionPath, testProjectPaths);
            Console.WriteLine($"Initial coverage run completed with exit code: {initialCoverage.ExitCode}");

            // Parse coverage
            Console.WriteLine("Parsing coverage data...");
            targetMap = await coverageParser.ParseCoverageAsync(initialCoverage.CoverageXmlPaths);
            Console.WriteLine($"Found {targetMap.Modules.Sum(m => m.Methods.Length)} uncovered methods");
        }

        // Main loop (simplified)
        var iteration = 0;
        while (iteration < iterationBudget && targetMap.Modules.Any(m => m.Methods.Length > 0))
        {
            iteration++;
            Console.WriteLine($"\nIteration {iteration}...");

            // Pick first method (simplified - would use priority rules)
            var methodTarget = targetMap.Modules
                .SelectMany(m => m.Methods)
                .FirstOrDefault();

            if (methodTarget == null) break;

            Console.WriteLine($"Targeting method: {methodTarget.MethodDisplayName}");

            try
            {
                // Collect context
                var generationRequest = await contextCollector.CollectContextAsync(
                    methodTarget,
                    new ReverseCoverage.TargetModel.Models.GenerationConstraints
                    {
                        MaxTestCases = 50, // Generate up to 50 tests
                        DeterministicOnly = true,
                        AllowedFrameworks = new[] { "xunit" }
                    });

                // Generate tests
                var generationResponse = await aiClient.GenerateAsync(generationRequest);
                
                // Log response details
                if (generationResponse.Notes != null && generationResponse.Notes.Length > 0)
                {
                    foreach (var note in generationResponse.Notes)
                    {
                        Console.WriteLine($"  Note: {note}");
                    }
                }
                
                Console.WriteLine($"  Response contains {generationResponse.ProposedTests.Length} test specifications");

                if (generationResponse.ProposedTests.Length > 0)
                {
                    Console.WriteLine($"Generated {generationResponse.ProposedTests.Length} test specifications");

                    // Determine output folder
                    var outputPath = !string.IsNullOrEmpty(outputFolder) 
                        ? outputFolder 
                        : Path.Combine(Path.GetDirectoryName(testProjectPaths[0]) ?? ".", "Generated");
                    
                    Directory.CreateDirectory(outputPath);
                    Console.WriteLine($"Writing tests to: {outputPath}");

                    // Emit tests to the specified folder
                    var testFiles = await EmitTestsToFolderAsync(
                        testEmitter,
                        generationResponse,
                        outputPath);

                    Console.WriteLine($"Emitted {testFiles.Length} test files");

                    // Verify (skip if generateAll mode)
                    if (!generateAll && initialCoverage != null)
                    {
                        var verificationResult = await verifier.VerifyTestsAsync(
                            testFiles,
                            initialCoverage,
                            solutionPath,
                            testProjectPaths);

                        if (verificationResult.Accepted)
                        {
                            Console.WriteLine("Tests accepted!");
                        }
                        else
                        {
                            Console.WriteLine($"Tests rejected: {verificationResult.RejectionReason}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Tests generated (verification skipped in generate-all mode)");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in iteration {iteration}: {ex.Message}");
            }
        }

        Console.WriteLine("\nOrchestration complete.");
    }

    private static async Task<string[]> EmitTestsToFolderAsync(
        TestEmitter.TestEmitter testEmitter,
        ReverseCoverage.TargetModel.Models.GenerationResponse response,
        string outputFolder)
    {
        var generatedFiles = new List<string>();
        Directory.CreateDirectory(outputFolder);

        // Group tests by target type
        var testsByType = response.ProposedTests
            .GroupBy(t => t.TargetMethodId.Split('|').FirstOrDefault() ?? "Unknown")
            .ToList();

        foreach (var group in testsByType)
        {
            var typeName = SanitizeTypeName(group.Key);
            var className = $"{typeName}Tests";
            var fileName = Path.Combine(outputFolder, $"{className}.cs");

            var testCode = GenerateTestClass(className, group.ToArray());
            await File.WriteAllTextAsync(fileName, testCode);
            generatedFiles.Add(fileName);
        }

        return generatedFiles.ToArray();
    }

    private static string GenerateTestClass(string className, ReverseCoverage.TargetModel.Models.TestCaseSpec[] testCases)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("// AUTO-GENERATED by Reverse Coverage Test Synthesis");
        sb.AppendLine("// Generated using Ollama local LLM");
        sb.AppendLine("using System;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine();

        // Extract namespace from first test's target method
        var namespaceName = "Generated.Tests";
        if (testCases.Length > 0 && testCases[0].TargetMethodId.Contains('|'))
        {
            var parts = testCases[0].TargetMethodId.Split('|');
            if (parts.Length > 0)
            {
                var typeParts = parts[0].Split('.');
                if (typeParts.Length > 1)
                {
                    namespaceName = string.Join(".", typeParts.Take(typeParts.Length - 1)) + ".Tests";
                }
            }
        }

        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var testCase in testCases)
        {
            var methodName = SanitizeMethodName(testCase.TestName);
            
            sb.AppendLine("        [Fact]");
            sb.AppendLine($"        public void {methodName}()");
            sb.AppendLine("        {");
            
            // Use the parsed test specification directly
            // The OllamaProvider already parsed TEST:/ARRANGE:/ACT:/ASSERT: format
            foreach (var step in testCase.Arrange)
            {
                if (!string.IsNullOrWhiteSpace(step.Text))
                {
                    // If it looks like code, use it directly; otherwise add as comment
                    if (step.Text.Contains('(') || step.Text.Contains('=') || step.Text.Contains("var "))
                    {
                        sb.AppendLine($"            {step.Text}");
                    }
                    else
                    {
                        sb.AppendLine($"            // Arrange: {step.Text}");
                    }
                }
            }
            
            if (!string.IsNullOrWhiteSpace(testCase.Act.Text))
            {
                // Act code
                if (testCase.Act.Text.Contains('(') || testCase.Act.Text.Contains('='))
                {
                    sb.AppendLine($"            {testCase.Act.Text}");
                }
                else
                {
                    sb.AppendLine($"            // Act: {testCase.Act.Text}");
                }
            }
            
            foreach (var assertion in testCase.Assert)
            {
                if (!string.IsNullOrWhiteSpace(assertion.Text))
                {
                    // Assert code
                    if (assertion.Text.Contains("Assert.") || assertion.Text.Contains("Throws") || assertion.Text.Contains("Equal"))
                    {
                        sb.AppendLine($"            {assertion.Text}");
                    }
                    else
                    {
                        sb.AppendLine($"            // Assert: {assertion.Text}");
                    }
                }
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string SanitizeTypeName(string name)
    {
        return new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
    }

    private static string SanitizeMethodName(string name)
    {
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return sanitized.Length > 0 ? sanitized : "Test";
    }
}

