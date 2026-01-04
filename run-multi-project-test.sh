#!/bin/bash
# Multi-Project Feasibility Test Runner

set -e

WORKSPACE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$WORKSPACE_ROOT"

CONFIG_FILE="${1:-multi-project-config.json}"

echo "═══════════════════════════════════════════════════════"
echo "   MULTI-PROJECT FEASIBILITY TEST RUNNER"
echo "═══════════════════════════════════════════════════════"
echo ""
echo "Config: $CONFIG_FILE"
echo "Workspace: $WORKSPACE_ROOT"
echo ""

# Build the test runner
echo "Building test runner..."
dotnet build tests/FeasibilityTestRunner.csproj --configuration Release --verbosity quiet

if [ $? -ne 0 ]; then
    echo "✗ Build failed"
    exit 1
fi
echo "✓ Build successful"
echo ""

# Run the multi-project test
dotnet run --project tests/FeasibilityTestRunner.csproj --no-build --configuration Release -- "$CONFIG_FILE"

EXIT_CODE=$?

echo ""
echo "═══════════════════════════════════════════════════════"
if [ $EXIT_CODE -eq 0 ]; then
    echo "✓ MULTI-PROJECT TEST PASSED"
else
    echo "✗ MULTI-PROJECT TEST FAILED"
fi
echo "═══════════════════════════════════════════════════════"

exit $EXIT_CODE
