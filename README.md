# ReverseCoverage Test Synthesis Framework

![Build Status](https://github.com/YOUR_USERNAME/YOUR_REPO/workflows/Build%20and%20Release/badge.svg)

A comprehensive .NET framework for automated test synthesis using reverse coverage analysis and AI-powered test generation.

## Overview

ReverseCoverage is a modular framework that analyzes code coverage, identifies untested code paths, and leverages AI to automatically generate unit tests. The framework consists of multiple components working together to streamline test creation and improve code coverage.

## Components

- **ReverseCoverage.TargetModel** - Core data models and structures
- **ReverseCoverage.CoverageRunner** - Coverage collection and execution
- **ReverseCoverage.CoverageParser** - Parses coverage reports (Cobertura, OpenCover)
- **ReverseCoverage.ContextCollector** - Collects and analyzes code context
- **ReverseCoverage.PluginContracts** - Plugin interfaces and contracts
- **ReverseCoverage.AiTestSynthesis** - AI-powered test generation
- **ReverseCoverage.TestEmitter** - Generates test code files
- **ReverseCoverage.Verifier** - Validates generated tests
- **ReverseCoverage.Orchestrator** - Main orchestration engine

## Requirements

- .NET 8.0 SDK or later
- xUnit test framework

## Building

```bash
# Restore dependencies
dotnet restore ReverseCoverage.sln

# Build the solution
dotnet build ReverseCoverage.sln --configuration Release

# Run tests
dotnet test ReverseCoverage.sln --configuration Release
```

## CI/CD

This project uses GitHub Actions for automated builds and releases.

### Automated Builds

Every push to `main` and all pull requests trigger:
- Dependency restoration
- Full solution build
- Test execution
- Test result reporting
- Artifact publishing (main branch only)

### Automated Releases

To create a new release:

1. **Create and push a version tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **The workflow will automatically:**
   - Build the solution
   - Run all tests
   - Package the Orchestrator and CoverageRunner as single-file executables
   - Generate release notes from commit history
   - Create a GitHub Release
   - Attach zip files for download

### Release Artifacts

Each release includes:
- `ReverseCoverage-Orchestrator-vX.X.X.zip` - Main orchestration engine
- `ReverseCoverage-CoverageRunner-vX.X.X.zip` - Coverage collection tool

## Usage

(Add usage instructions here once documentation is available)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

All pull requests will be automatically built and tested.

## License

(Add license information here)
