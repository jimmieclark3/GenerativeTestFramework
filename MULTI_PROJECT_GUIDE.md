# Multi-Project Feasibility Testing Guide

## Overview

Test your Reverse Coverage framework against **multiple codebases** simultaneously to prove it works across different projects, codebases, and complexity levels.

## Quick Start

### 1. Configure Projects

Edit `multi-project-config.json`:

```json
{
  "projects": [
    {
      "name": "DemoCalc",
      "description": "Simple calculator",
      "solutionPath": "ReverseCoverage.sln",
      "testProjectPaths": ["src/DemoCalc.Tests/DemoCalc.Tests.csproj"],
      "coverageThreshold": 80,
      "iterationBudget": 5,
      "enabled": true
    },
    {
      "name": "YourApp",
      "description": "Your production app",
      "gitRepo": "https://github.com/you/yourapp.git",
      "clonePath": "external/YourApp",
      "solutionPath": "external/YourApp/YourApp.sln",
      "testProjectPaths": ["external/YourApp/tests/YourApp.Tests.csproj"],
      "coverageThreshold": 70,
      "iterationBudget": 10,
      "enabled": true
    }
  ],
  "executionMode": "sequential",
  "parallelismDegree": 2,
  "generateComparativeReport": true
}
```

### 2. Run Tests

```bash
./run-multi-project-test.sh
```

Or specify a custom config:

```bash
./run-multi-project-test.sh my-config.json
```

## Configuration Options

### Project Settings

| Field | Description | Example |
|-------|-------------|---------|
| `name` | Project identifier | `"DemoCalc"` |
| `description` | Human-readable description | `"Calculator demo"` |
| `gitRepo` | Git URL (for external projects) | `"https://github.com/..."` |
| `clonePath` | Where to clone external repos | `"external/ProjectName"` |
| `solutionPath` | Path to .sln file | `"MyApp.sln"` |
| `testProjectPaths` | Test project paths (array) | `["tests/Tests.csproj"]` |
| `coverageThreshold` | Target coverage % | `80` |
| `iterationBudget` | Max test generation iterations | `10` |
| `enabled` | Include in test run | `true` |

### Execution Settings

| Field | Description | Values |
|-------|-------------|--------|
| `executionMode` | Run sequentially or in parallel | `"sequential"` or `"parallel"` |
| `parallelismDegree` | How many projects in parallel | `2` (recommended) |
| `generateComparativeReport` | Create HTML report | `true` |
| `stopOnFirstFailure` | Stop if any project fails | `false` |

## Execution Modes

### Sequential Mode (Recommended)
```json
"executionMode": "sequential"
```
- Projects run one after another
- Easier to debug
- Full output for each project
- Lower resource usage

### Parallel Mode
```json
"executionMode": "parallel",
"parallelismDegree": 2
```
- Multiple projects run simultaneously
- Faster overall execution
- Higher resource usage
- Use for large test suites

## What Gets Tested

For each enabled project:

1. **Clone** - External repos cloned automatically
2. **Build** - Solution compiled
3. **Baseline** - Coverage measured before generation
4. **Generate** - Orchestrator creates tests
5. **Improved** - Coverage measured after generation
6. **Metrics** - Quality scores calculated

## Output & Reports

### Console Output
```
═══════════════════════════════════════════════════════
   MULTI-PROJECT FEASIBILITY TEST
═══════════════════════════════════════════════════════

Testing 3 project(s):
  • DemoCalc - Simple calculator
  • Humanizer - String manipulation library
  • MyApp - Production application

[DemoCalc] Building...
  ✓ Build successful
[DemoCalc] Measuring baseline coverage...
  ✓ Baseline: 0.0% line coverage
[DemoCalc] Running test generation...
  ✓ Generated 10 tests
[DemoCalc] Measuring improved coverage...
  ✓ Improved: 85.2% line coverage

[DemoCalc] Results:
  Coverage improvement: +85.2%
  Quality score: 91.4%
  Duration: 23.5s

... (repeat for each project) ...

═══════════════════════════════════════════════════════
   MULTI-PROJECT TEST SUMMARY
═══════════════════════════════════════════════════════

Total Projects:  3
Successful:      3
Failed:          0
Total Duration:  2.3 minutes

Results by Project:
  ✓ DemoCalc              | Improvement: +85.2% | Score: 91.4%
  ✓ Humanizer             | Improvement: +12.3% | Score: 78.5%
  ✓ MyApp                 | Improvement: +45.7% | Score: 82.1%

Overall: ✓ PASS
═══════════════════════════════════════════════════════
```

### Generated Files

```
artifacts/multi-project-feasibility/
├── DemoCalc/
│   ├── baseline/
│   │   └── coverage.cobertura.xml
│   └── improved/
│       └── coverage.cobertura.xml
├── Humanizer/
│   ├── baseline/...
│   └── improved/...
├── MyApp/
│   ├── baseline/...
│   └── improved/...
├── comparative-report.html          ← Beautiful HTML report
└── multi-project-results.json       ← Full JSON results
```

### HTML Comparative Report

Open `artifacts/multi-project-feasibility/comparative-report.html` in a browser:

- Side-by-side comparison table
- Coverage improvement metrics
- Quality scores
- Duration statistics
- Color-coded success/failure
- Aggregate statistics

## Example Configs

### Test DemoCalc Only
```json
{
  "projects": [
    {
      "name": "DemoCalc",
      "solutionPath": "ReverseCoverage.sln",
      "testProjectPaths": ["src/DemoCalc.Tests/DemoCalc.Tests.csproj"],
      "enabled": true
    }
  ],
  "executionMode": "sequential"
}
```

### Test Multiple External Projects
```json
{
  "projects": [
    {
      "name": "DemoCalc",
      "solutionPath": "ReverseCoverage.sln",
      "testProjectPaths": ["src/DemoCalc.Tests/DemoCalc.Tests.csproj"],
      "enabled": true
    },
    {
      "name": "Humanizer",
      "gitRepo": "https://github.com/Humanizr/Humanizer.git",
      "clonePath": "external/Humanizer",
      "solutionPath": "external/Humanizer/Humanizer.sln",
      "testProjectPaths": ["external/Humanizer/src/Humanizer.Tests/Humanizer.Tests.csproj"],
      "enabled": true
    },
    {
      "name": "Newtonsoft.Json",
      "gitRepo": "https://github.com/JamesNK/Newtonsoft.Json.git",
      "clonePath": "external/Newtonsoft.Json",
      "solutionPath": "external/Newtonsoft.Json/Src/Newtonsoft.Json.sln",
      "testProjectPaths": ["external/Newtonsoft.Json/Src/Newtonsoft.Json.Tests/Newtonsoft.Json.Tests.csproj"],
      "enabled": true
    }
  ],
  "executionMode": "parallel",
  "parallelismDegree": 2
}
```

## Troubleshooting

### Project Won't Clone
- Check git URL is correct
- Ensure you have network access
- Try cloning manually first

### Build Fails
- Check solution path is correct
- Ensure dependencies are available
- Test project paths must exist

### No Coverage Improvement
- Check MockAI provider is working
- Verify tests are being generated
- Look at orchestrator output for errors

## Adding Your Own Projects

1. **Local Project:**
   ```json
   {
     "name": "MyLocalApp",
     "solutionPath": "../my-local-app/App.sln",
     "testProjectPaths": ["../my-local-app/tests/Tests.csproj"],
     "enabled": true
   }
   ```

2. **External Open-Source:**
   ```json
   {
     "name": "ExternalLib",
     "gitRepo": "https://github.com/org/repo.git",
     "clonePath": "external/ExternalLib",
     "solutionPath": "external/ExternalLib/Lib.sln",
     "testProjectPaths": ["external/ExternalLib/tests/Tests.csproj"],
     "enabled": true
   }
   ```

## Best Practices

1. **Start Small**: Test DemoCalc first to verify setup
2. **One at a Time**: Enable projects incrementally
3. **Use Sequential**: Parallel mode for final runs only
4. **Check Paths**: Verify all paths before running
5. **Review Reports**: Check HTML report for insights

## Integration with CI/CD

Add to `.github/workflows/build-release.yml`:

```yaml
- name: Multi-Project Feasibility Test
  run: |
    chmod +x run-multi-project-test.sh
    ./run-multi-project-test.sh multi-project-config.json
```

## Success Metrics

**Pass Criteria:**
- All enabled projects build successfully
- Coverage improves (Δ > 0%) for each project
- Quality score ≥ 70% average across projects
- All generated tests compile and run

**What Success Proves:**
- ✅ Framework works across different codebases
- ✅ Test generation scales to real projects
- ✅ Quality is consistent and measurable
- ✅ Framework is production-ready

---

**Ready to test multiple projects and prove your framework works at scale!** 🚀
