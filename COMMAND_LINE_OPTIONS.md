# Command-Line Options Reference

This document provides a comprehensive reference for all command-line options available in the Test Synthesis Framework orchestrator.

## Usage

```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path <path> \
  --test-project-path <path> [<path>...] \
  [OPTIONS]
```

Or use a configuration file:

```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path <path> \
  --test-project-path <path> \
  --config config.json \
  [OPTIONS]
```

Command-line options override values from the configuration file.

## Required Options

### `--solution-path <path>`
Path to the solution (.sln) or project (.csproj) file.

**Example:** `--solution-path ReverseCoverage.sln`

### `--test-project-path <path> [<path>...]`
Path(s) to one or more test project(s). Multiple paths can be specified.

**Example:** `--test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj`

## Configuration File

### `--config <path>`
Path to a JSON configuration file containing default options. Command-line arguments override values from the config file.

**Example:** `--config config.json`

See `config.example.json` for a complete example configuration file.

## Coverage Thresholds

### `--line-coverage-threshold <percentage>`
Target line coverage percentage (0-100). Default: `100`

**Example:** `--line-coverage-threshold 90`

### `--branch-coverage-threshold <percentage>`
Target branch coverage percentage (0-100). Default: `100`

**Example:** `--branch-coverage-threshold 85`

### `--overall-coverage-threshold <percentage>`
Target overall coverage percentage (0-100). Default: `100`

**Example:** `--overall-coverage-threshold 88`

### `--stop-on-any-threshold`
Stop when any threshold is met (vs requiring all thresholds). Default: `false`

**Example:** `--stop-on-any-threshold`

## Test Generation Limits

### `--max-tests-per-method <count>`
Maximum number of test cases to generate per method. Default: `10`

**Example:** `--max-tests-per-method 15`

### `--max-total-tests <count>`
Maximum total number of tests to generate across all methods. Default: `1000`

**Example:** `--max-total-tests 2000`

### `--max-iterations <count>`
Maximum number of iterations (methods to process). Default: `100`

**Example:** `--max-iterations 200`

### `--time-budget-minutes <minutes>`
Maximum time budget in minutes. Default: `60`

**Example:** `--time-budget-minutes 120`

### `--cost-budget <dollars>`
Maximum cost budget in dollars for AI API calls. Default: `10.00`

**Example:** `--cost-budget 25.00`

## Test Framework Settings

### `--test-framework <framework>`
Test framework to use: `xunit`, `nunit`, `mstest`. Default: `xunit`

**Example:** `--test-framework xunit`

### `--mocking-framework <framework>`
Mocking framework: `Moq`, `NSubstitute`, `None`. Default: `Moq`

**Example:** `--mocking-framework Moq`

### `--assertion-style <style>`
Assertion style: `BuiltIn`, `FluentAssertions`. Default: `BuiltIn`

**Example:** `--assertion-style FluentAssertions`

### `--use-theory`
Use Theory/InlineData for parameterized tests. Default: `true`

**Example:** `--use-theory`

### `--no-use-theory`
Disable Theory/InlineData for parameterized tests.

## Test Naming and Organization

### `--test-naming <pattern>`
Test naming convention pattern. Default: `{Type}_{Method}_Should_{Behavior}`

**Example:** `--test-naming {Type}_{Method}_Should_{Behavior}`

### `--output-layout <layout>`
Output layout: `per-type` (one file per class), `per-method` (one file per method). Default: `per-type`

**Example:** `--output-layout per-type`

### `--generated-test-namespace <namespace>`
Namespace for generated tests. If not specified, uses `{OriginalNamespace}.Tests.Generated`

**Example:** `--generated-test-namespace MyApp.Tests.Generated`

### `--generated-test-subdirectory <directory>`
Subdirectory for generated tests within test project. Default: `Generated`

**Example:** `--generated-test-subdirectory Generated`

### `--test-class-prefix <prefix>`
Prefix for generated test class names. Default: empty

**Example:** `--test-class-prefix Auto`

### `--test-class-suffix <suffix>`
Suffix for generated test class names. Default: `Tests`

**Example:** `--test-class-suffix Tests`

## Test Content Control

### `--include-comments`
Include comments in generated tests. Default: `true`

### `--no-include-comments`
Exclude comments from generated tests.

### `--include-aaa-comments`
Include arrange/act/assert comments. Default: `true`

### `--no-include-aaa-comments`
Exclude arrange/act/assert comments.

### `--include-private-methods`
Generate tests for private methods (requires InternalsVisibleTo). Default: `false`

### `--include-internal-methods`
Generate tests for internal methods. Default: `true`

### `--no-include-internal-methods`
Exclude internal methods from test generation.

### `--include-static-methods`
Generate tests for static methods. Default: `true`

### `--no-include-static-methods`
Exclude static methods from test generation.

### `--include-async-methods`
Generate tests for async methods. Default: `true`

### `--no-include-async-methods`
Exclude async methods from test generation.

### `--include-generic-methods`
Generate tests for generic methods. Default: `true`

### `--no-include-generic-methods`
Exclude generic methods from test generation.

### `--include-extension-methods`
Generate tests for extension methods. Default: `true`

### `--no-include-extension-methods`
Exclude extension methods from test generation.

### `--include-properties`
Generate tests for properties. Default: `false`

### `--include-constructors`
Generate tests for constructors. Default: `false`

### `--min-method-complexity <complexity>`
Minimum method complexity (cyclomatic complexity) to target. Default: `1`

**Example:** `--min-method-complexity 3`

### `--max-method-complexity <complexity>`
Maximum method complexity to target. Default: `1000` (unlimited)

**Example:** `--max-method-complexity 50`

## Test Quality Settings

### `--min-test-pass-rate <percentage>`
Minimum test pass rate required to accept tests (0-100). Default: `100`

**Example:** `--min-test-pass-rate 95`

### `--require-deterministic`
Require deterministic tests only (no time/random dependencies). Default: `true`

### `--no-require-deterministic`
Allow non-deterministic tests.

### `--min-coverage-improvement <percentage>`
Minimum coverage improvement required to accept tests (percentage points). Default: `0.1`

**Example:** `--min-coverage-improvement 0.5`

### `--determinism-check-runs <count>`
Number of times to run tests to verify determinism. Default: `3`

**Example:** `--determinism-check-runs 5`

### `--require-compilation-success`
Require all generated tests to compile. Default: `true`

### `--no-require-compilation-success`
Allow tests that don't compile (for debugging).

## Filtering and Targeting

### `--include-pattern <pattern> [<pattern>...]`
Include patterns for methods/types (glob patterns). Can be specified multiple times.

**Example:** `--include-pattern "*Service*" --include-pattern "*Helper*"`

### `--exclude-pattern <pattern> [<pattern>...]`
Exclude patterns for methods/types (glob patterns). Can be specified multiple times.

**Example:** `--exclude-pattern "*Generated*" --exclude-pattern "*Designer*"`

### `--exclude-attribute <attribute> [<attribute>...]`
Exclude methods with these attributes. Can be specified multiple times.

**Example:** `--exclude-attribute Obsolete --exclude-attribute EditorBrowsable`

### `--exclude-file-pattern <pattern> [<pattern>...]`
Exclude files matching these patterns. Can be specified multiple times.

**Example:** `--exclude-file-pattern "*.Designer.cs"`

### `--target-namespace <namespace> [<namespace>...]`
Only target methods in these namespaces. Can be specified multiple times.

**Example:** `--target-namespace MyApp.Services --target-namespace MyApp.Helpers`

### `--target-assembly <assembly> [<assembly>...]`
Only target methods in these assemblies. Can be specified multiple times.

**Example:** `--target-assembly MyApp.Core`

### `--method-priority <priority> [<priority>...]`
Priority order for method selection: `Branches`, `Lines`, `Complexity`, `Size`. Default: `Branches`, `Lines`

**Example:** `--method-priority Branches --method-priority Lines --method-priority Complexity`

## AI Provider Settings

### `--ai-provider <provider>`
AI provider: `MockAi`, `Claude`, `OpenAI`, `LocalLlamaCpp`, `Ollama`, `CustomHttp`. Default: `MockAi`

**Example:** `--ai-provider Ollama`

### `--ai-model <model>`
AI model name (provider-specific). Uses provider default if not specified.

**Example:** `--ai-model claude-3-5-sonnet-20241022`

### `--ai-api-key <key>`
AI API key. If not specified, uses the `AI_API_KEY` environment variable.

**Example:** `--ai-api-key sk-ant-api03-...`

### `--ai-base-url <url>`
AI base URL (for custom providers).

**Example:** `--ai-base-url https://api.example.com/v1`

### `--ai-temperature <temperature>`
AI temperature (0.0-2.0). Lower = more deterministic. Default: `0.0`

**Example:** `--ai-temperature 0.2`

### `--ai-max-tokens <tokens>`
AI max output tokens. Default: `2000`

**Example:** `--ai-max-tokens 4000`

### `--ai-timeout-seconds <seconds>`
AI request timeout in seconds. Default: `300`

**Example:** `--ai-timeout-seconds 600`

### `--ai-max-retries <count>`
AI max retries on failure. Default: `3`

**Example:** `--ai-max-retries 5`

### `--local-model-path <path>`
Local model path (for LocalLlamaCpp).

**Example:** `--local-model-path C:\Models\llama-2-7b.gguf`

## Output and Logging

### `--output-directory <directory>`
Output directory for artifacts. Default: `artifacts`

**Example:** `--output-directory output`

### `--verbosity <level>`
Log verbosity: `Quiet`, `Minimal`, `Normal`, `Detailed`, `Diagnostic`. Default: `Normal`

**Example:** `--verbosity Detailed`

### `--save-request-responses`
Save request/response JSON files for debugging. Default: `true`

### `--no-save-request-responses`
Don't save request/response JSON files.

### `--generate-html-report`
Generate HTML coverage report. Default: `false`

### `--generate-json-report`
Generate JSON summary report. Default: `true`

### `--no-generate-json-report`
Don't generate JSON summary report.

### `--generate-markdown-report`
Generate markdown summary report. Default: `true`

### `--no-generate-markdown-report`
Don't generate markdown summary report.

### `--show-progress`
Show progress bar. Default: `true`

### `--no-show-progress`
Hide progress bar.

### `--color-output`
Color output (if supported). Default: `true`

### `--no-color-output`
Disable colored output.

## Advanced Settings

### `--parallel-generation`
Enable parallel test generation (multiple methods simultaneously). Default: `false`

**Example:** `--parallel-generation`

### `--max-parallel-generations <count>`
Maximum number of parallel generations. Default: `4`

**Example:** `--max-parallel-generations 8`

### `--cache-workspace`
Cache Roslyn workspace between iterations. Default: `true`

### `--no-cache-workspace`
Don't cache Roslyn workspace.

### `--skip-complex-dependencies`
Skip methods that require complex dependencies. Default: `false`

### `--infeasible-branch-attempts <count>`
Mark infeasible branches after N failed attempts. Default: `5`

**Example:** `--infeasible-branch-attempts 10`

### `--continue-on-error`
Continue on errors (don't stop on first failure). Default: `true`

### `--no-continue-on-error`
Stop on first error.

### `--dry-run`
Dry run mode (don't actually generate tests). Useful for testing configuration. Default: `false`

**Example:** `--dry-run`

## Examples

### Basic Usage
```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --line-coverage-threshold 90 \
  --max-iterations 50
```

### Using Configuration File
```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --config config.json \
  --line-coverage-threshold 95
```

### Advanced Filtering
```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --target-namespace MyApp.Services \
  --exclude-pattern "*Generated*" \
  --exclude-attribute Obsolete \
  --include-private-methods \
  --max-tests-per-method 20
```

### AI Provider Configuration

**Claude:**
```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --ai-provider Claude \
  --ai-model claude-3-5-sonnet-20241022 \
  --ai-temperature 0.1 \
  --ai-max-tokens 4000 \
  --cost-budget 50.00
```

**Ollama (Local LLM):**
```bash
# First, make sure Ollama is running and you've pulled a model:
# ollama pull llama3.2:3b

dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --ai-provider Ollama \
  --ai-model llama3.2:3b \
  --ai-base-url http://localhost:11434 \
  --ai-temperature 0.0 \
  --ai-max-tokens 2000
```

### Dry Run
```bash
dotnet run --project src/ReverseCoverage.Orchestrator -- \
  --solution-path ReverseCoverage.sln \
  --test-project-path src/DemoCalc.Tests/DemoCalc.Tests.csproj \
  --dry-run \
  --verbosity Detailed
```


