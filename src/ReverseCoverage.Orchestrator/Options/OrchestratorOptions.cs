// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace ReverseCoverage.Orchestrator.Options;

/// <summary>
/// Comprehensive options for controlling test generation and coverage thresholds.
/// </summary>
public class OrchestratorOptions
{
    // ========== Coverage Thresholds ==========
    
    /// <summary>
    /// Target line coverage percentage (0-100). Default: 100
    /// </summary>
    public int LineCoverageThreshold { get; set; } = 100;

    /// <summary>
    /// Target branch coverage percentage (0-100). Default: 100
    /// </summary>
    public int BranchCoverageThreshold { get; set; } = 100;

    /// <summary>
    /// Target overall coverage percentage (0-100). Default: 100
    /// </summary>
    public int OverallCoverageThreshold { get; set; } = 100;

    /// <summary>
    /// Stop when any threshold is met (true) or require all thresholds (false). Default: false
    /// </summary>
    public bool StopOnAnyThreshold { get; set; } = false;

    // ========== Test Generation Limits ==========

    /// <summary>
    /// Maximum number of test cases to generate per method. Default: 10
    /// </summary>
    public int MaxTestsPerMethod { get; set; } = 10;

    /// <summary>
    /// Maximum total number of tests to generate across all methods. Default: 1000
    /// </summary>
    public int MaxTotalTests { get; set; } = 1000;

    /// <summary>
    /// Maximum number of iterations (methods to process). Default: 100
    /// </summary>
    public int MaxIterations { get; set; } = 100;

    /// <summary>
    /// Maximum time budget in minutes. Default: 60
    /// </summary>
    public int TimeBudgetMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum cost budget in dollars. Default: 10.00
    /// </summary>
    public decimal CostBudget { get; set; } = 10.00m;

    // ========== Test Framework Settings ==========

    /// <summary>
    /// Test framework to use: xunit, nunit, mstest. Default: xunit
    /// </summary>
    public string TestFramework { get; set; } = "xunit";

    /// <summary>
    /// Mocking framework: Moq, NSubstitute, None. Default: Moq
    /// </summary>
    public string MockingFramework { get; set; } = "Moq";

    /// <summary>
    /// Assertion style: BuiltIn, FluentAssertions. Default: BuiltIn
    /// </summary>
    public string AssertionStyle { get; set; } = "BuiltIn";

    /// <summary>
    /// Use Theory/InlineData for parameterized tests. Default: true
    /// </summary>
    public bool UseTheoryForParameterized { get; set; } = true;

    // ========== Test Naming and Organization ==========

    /// <summary>
    /// Test naming convention pattern. Default: "{Type}_{Method}_Should_{Behavior}"
    /// </summary>
    public string TestNamingConvention { get; set; } = "{Type}_{Method}_Should_{Behavior}";

    /// <summary>
    /// Output layout: per-type (one file per class), per-method (one file per method). Default: per-type
    /// </summary>
    public string OutputLayout { get; set; } = "per-type";

    /// <summary>
    /// Namespace for generated tests. Default: {OriginalNamespace}.Tests.Generated
    /// </summary>
    public string? GeneratedTestNamespace { get; set; }

    /// <summary>
    /// Subdirectory for generated tests within test project. Default: Generated
    /// </summary>
    public string GeneratedTestSubdirectory { get; set; } = "Generated";

    /// <summary>
    /// Prefix for generated test class names. Default: empty
    /// </summary>
    public string TestClassPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Suffix for generated test class names. Default: Tests
    /// </summary>
    public string TestClassSuffix { get; set; } = "Tests";

    // ========== Test Content Control ==========

    /// <summary>
    /// Include comments in generated tests. Default: true
    /// </summary>
    public bool IncludeComments { get; set; } = true;

    /// <summary>
    /// Include arrange/act/assert comments. Default: true
    /// </summary>
    public bool IncludeArrangeActAssertComments { get; set; } = true;

    /// <summary>
    /// Generate tests for private methods (requires InternalsVisibleTo). Default: false
    /// </summary>
    public bool IncludePrivateMethods { get; set; } = false;

    /// <summary>
    /// Generate tests for internal methods. Default: true
    /// </summary>
    public bool IncludeInternalMethods { get; set; } = true;

    /// <summary>
    /// Generate tests for static methods. Default: true
    /// </summary>
    public bool IncludeStaticMethods { get; set; } = true;

    /// <summary>
    /// Generate tests for async methods. Default: true
    /// </summary>
    public bool IncludeAsyncMethods { get; set; } = true;

    /// <summary>
    /// Generate tests for generic methods. Default: true
    /// </summary>
    public bool IncludeGenericMethods { get; set; } = true;

    /// <summary>
    /// Generate tests for extension methods. Default: true
    /// </summary>
    public bool IncludeExtensionMethods { get; set; } = true;

    /// <summary>
    /// Generate tests for properties. Default: false
    /// </summary>
    public bool IncludeProperties { get; set; } = false;

    /// <summary>
    /// Generate tests for constructors. Default: false
    /// </summary>
    public bool IncludeConstructors { get; set; } = false;

    /// <summary>
    /// Minimum method complexity to target (cyclomatic complexity). Default: 1
    /// </summary>
    public int MinMethodComplexity { get; set; } = 1;

    /// <summary>
    /// Maximum method complexity to target. Default: 1000 (unlimited)
    /// </summary>
    public int MaxMethodComplexity { get; set; } = 1000;

    // ========== Test Quality Settings ==========

    /// <summary>
    /// Minimum test pass rate required to accept tests (0-100). Default: 100
    /// </summary>
    public int MinTestPassRate { get; set; } = 100;

    /// <summary>
    /// Require deterministic tests only (no time/random dependencies). Default: true
    /// </summary>
    public bool RequireDeterministicTests { get; set; } = true;

    /// <summary>
    /// Minimum coverage improvement required to accept tests (percentage points). Default: 0.1
    /// </summary>
    public double MinCoverageImprovement { get; set; } = 0.1;

    /// <summary>
    /// Run tests multiple times to verify determinism. Default: 3
    /// </summary>
    public int DeterminismCheckRuns { get; set; } = 3;

    /// <summary>
    /// Require all generated tests to compile. Default: true
    /// </summary>
    public bool RequireCompilationSuccess { get; set; } = true;

    // ========== Filtering and Targeting ==========

    /// <summary>
    /// Include patterns for methods/types (glob patterns). Default: empty (include all)
    /// </summary>
    public string[] IncludePatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Exclude patterns for methods/types (glob patterns). Default: empty
    /// </summary>
    public string[] ExcludePatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Exclude methods with these attributes. Default: empty
    /// </summary>
    public string[] ExcludeAttributes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Exclude files matching these patterns. Default: empty
    /// </summary>
    public string[] ExcludeFilePatterns { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Only target methods in these namespaces. Default: empty (all namespaces)
    /// </summary>
    public string[] TargetNamespaces { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Only target methods in these assemblies. Default: empty (all assemblies)
    /// </summary>
    public string[] TargetAssemblies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Priority order for method selection: Branches, Lines, Complexity, Size. Default: Branches, Lines
    /// </summary>
    public string[] MethodPriorityOrder { get; set; } = new[] { "Branches", "Lines" };

    // ========== AI Provider Settings ==========

    /// <summary>
    /// AI provider: MockAi, Claude, OpenAI, LocalLlamaCpp, Ollama, CustomHttp. Default: MockAi
    /// </summary>
    public string AiProvider { get; set; } = "MockAi";

    /// <summary>
    /// AI model name (provider-specific). Default: provider default
    /// </summary>
    public string? AiModel { get; set; }

    /// <summary>
    /// AI API key (from environment if not set). Default: null
    /// </summary>
    public string? AiApiKey { get; set; }

    /// <summary>
    /// AI base URL (for custom providers). Default: null
    /// </summary>
    public string? AiBaseUrl { get; set; }

    /// <summary>
    /// AI temperature (0.0-2.0). Lower = more deterministic. Default: 0.0
    /// </summary>
    public double AiTemperature { get; set; } = 0.0;

    /// <summary>
    /// AI max output tokens. Default: 2000
    /// </summary>
    public int AiMaxTokens { get; set; } = 2000;

    /// <summary>
    /// AI request timeout in seconds. Default: 300
    /// </summary>
    public int AiTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// AI max retries on failure. Default: 3
    /// </summary>
    public int AiMaxRetries { get; set; } = 3;

    /// <summary>
    /// Local model path (for LocalLlamaCpp). Default: null
    /// </summary>
    public string? LocalModelPath { get; set; }

    // ========== Output and Logging ==========

    /// <summary>
    /// Output directory for artifacts. Default: artifacts
    /// </summary>
    public string OutputDirectory { get; set; } = "artifacts";

    /// <summary>
    /// Log verbosity: Quiet, Minimal, Normal, Detailed, Diagnostic. Default: Normal
    /// </summary>
    public string LogVerbosity { get; set; } = "Normal";

    /// <summary>
    /// Save request/response JSON files for debugging. Default: true
    /// </summary>
    public bool SaveRequestResponses { get; set; } = true;

    /// <summary>
    /// Generate HTML coverage report. Default: false
    /// </summary>
    public bool GenerateHtmlReport { get; set; } = false;

    /// <summary>
    /// Generate JSON summary report. Default: true
    /// </summary>
    public bool GenerateJsonReport { get; set; } = true;

    /// <summary>
    /// Generate markdown summary report. Default: true
    /// </summary>
    public bool GenerateMarkdownReport { get; set; } = true;

    /// <summary>
    /// Show progress bar. Default: true
    /// </summary>
    public bool ShowProgress { get; set; } = true;

    /// <summary>
    /// Color output (if supported). Default: true
    /// </summary>
    public bool ColorOutput { get; set; } = true;

    // ========== Advanced Settings ==========

    /// <summary>
    /// Parallel test generation (multiple methods simultaneously). Default: false
    /// </summary>
    public bool ParallelGeneration { get; set; } = false;

    /// <summary>
    /// Max parallel generations. Default: 4
    /// </summary>
    public int MaxParallelGenerations { get; set; } = 4;

    /// <summary>
    /// Cache Roslyn workspace between iterations. Default: true
    /// </summary>
    public bool CacheWorkspace { get; set; } = true;

    /// <summary>
    /// Skip methods that require complex dependencies. Default: false
    /// </summary>
    public bool SkipComplexDependencies { get; set; } = false;

    /// <summary>
    /// Mark infeasible branches after N failed attempts. Default: 5
    /// </summary>
    public int InfeasibleBranchAttempts { get; set; } = 5;

    /// <summary>
    /// Continue on errors (don't stop on first failure). Default: true
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Dry run mode (don't actually generate tests). Default: false
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Configuration file path (JSON). Default: null
    /// </summary>
    public string? ConfigFile { get; set; }
}


