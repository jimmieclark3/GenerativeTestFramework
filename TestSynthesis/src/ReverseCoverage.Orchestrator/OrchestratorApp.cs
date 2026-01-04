// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.CommandLine;
using ReverseCoverage.AiTestSynthesis;
using ReverseCoverage.ContextCollector;
using ReverseCoverage.CoverageParser;
using ReverseCoverage.CoverageRunner;
using ReverseCoverage.CoverageRunner.Options;
using ReverseCoverage.PluginContracts;
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
            "AI provider: LocalLlamaCpp, OpenAIResponses, or CustomHttp");

        var rootCommand = new RootCommand("Reverse Coverage Test Synthesis Orchestrator")
        {
            solutionPathOption,
            testProjectPathOption,
            coverageThresholdOption,
            iterationBudgetOption,
            providerOption
        };

        rootCommand.SetHandler(async (solutionPath, testProjectPaths, coverageThreshold, iterationBudget, provider) =>
        {
            await RunOrchestratorAsync(solutionPath, testProjectPaths, coverageThreshold, iterationBudget, provider);
        },
        solutionPathOption, testProjectPathOption, coverageThresholdOption, iterationBudgetOption, providerOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task RunOrchestratorAsync(
        string solutionPath,
        string[] testProjectPaths,
        int coverageThreshold,
        int iterationBudget,
        string provider)
    {
        Console.WriteLine("Reverse Coverage Test Synthesis Orchestrator");
        Console.WriteLine($"Solution: {solutionPath}");
        Console.WriteLine($"Test Projects: {string.Join(", ", testProjectPaths)}");
        Console.WriteLine($"Coverage Threshold: {coverageThreshold}%");
        Console.WriteLine($"Iteration Budget: {iterationBudget}");
        Console.WriteLine($"Provider: {provider}");
        Console.WriteLine();

        // Initialize components
        var coverageRunner = new CoverageRunner.CoverageRunner(new CoverageRunnerOptions());
        var coverageParser = new CoverageParser.CoverageParser();
        var contextCollector = new ContextCollector.ContextCollector();
        var aiClient = new AiTestSynthesisClient(new AiTestSynthesisOptions
        {
            Provider = Enum.Parse<AiProvider>(provider, ignoreCase: true)
        });
        var testEmitter = new TestEmitter.TestEmitter();
        var verifier = new Verifier.Verifier();

        // Run initial coverage
        Console.WriteLine("Running initial coverage...");
        var initialCoverage = await coverageRunner.RunCoverageAsync(solutionPath, testProjectPaths);
        Console.WriteLine($"Initial coverage run completed with exit code: {initialCoverage.ExitCode}");

        // Parse coverage
        Console.WriteLine("Parsing coverage data...");
        var targetMap = await coverageParser.ParseCoverageAsync(initialCoverage.CoverageXmlPaths);
        Console.WriteLine($"Found {targetMap.Modules.Sum(m => m.Methods.Length)} uncovered methods");

        // Load solution for context collection
        await contextCollector.LoadSolutionAsync(solutionPath);

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
                        MaxTestCases = 10,
                        DeterministicOnly = true,
                        AllowedFrameworks = new[] { "xunit" }
                    });

                // Generate tests
                var generationResponse = await aiClient.GenerateAsync(generationRequest);

                if (generationResponse.ProposedTests.Length > 0)
                {
                    Console.WriteLine($"Generated {generationResponse.ProposedTests.Length} test specifications");

                    // Emit tests
                    var testFiles = await testEmitter.EmitTestsAsync(
                        generationResponse,
                        testProjectPaths[0]);

                    Console.WriteLine($"Emitted {testFiles.Length} test files");

                    // Verify
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in iteration {iteration}: {ex.Message}");
            }
        }

        Console.WriteLine("\nOrchestration complete.");
    }
}

