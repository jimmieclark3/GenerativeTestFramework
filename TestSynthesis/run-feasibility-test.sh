#!/bin/bash
# Reverse Coverage Feasibility Test Runner

set -e

echo "═══════════════════════════════════════════════════════"
echo "   REVERSE COVERAGE FRAMEWORK - FEASIBILITY TEST"
echo "═══════════════════════════════════════════════════════"
echo ""

# Set colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

WORKSPACE_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$WORKSPACE_ROOT"

echo "Workspace: $WORKSPACE_ROOT"
echo ""

# Step 1: Build all projects
echo "──────────────────────────────────────────────────────"
echo "STEP 1: Building all projects..."
echo "──────────────────────────────────────────────────────"
dotnet build ReverseCoverage.sln --configuration Debug --verbosity quiet
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓${NC} Build successful"
else
    echo -e "${RED}✗${NC} Build failed"
    exit 1
fi
echo ""

# Step 2: Measure baseline coverage (empty test)
echo "──────────────────────────────────────────────────────"
echo "STEP 2: Measuring baseline coverage..."
echo "──────────────────────────────────────────────────────"
mkdir -p artifacts/feasibility/baseline
dotnet test src/DemoCalc.Tests/DemoCalc.Tests.csproj \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput="../../artifacts/feasibility/baseline/" \
    --nologo --verbosity quiet

if [ -f "artifacts/feasibility/baseline/coverage.cobertura.xml" ]; then
    BASELINE_LINE=$(grep 'line-rate' artifacts/feasibility/baseline/coverage.cobertura.xml | head -1 | sed -n 's/.*line-rate="\([^"]*\)".*/\1/p')
    BASELINE_BRANCH=$(grep 'branch-rate' artifacts/feasibility/baseline/coverage.cobertura.xml | head -1 | sed -n 's/.*branch-rate="\([^"]*\)".*/\1/p')
    BASELINE_LINE_PCT=$(awk "BEGIN {printf \"%.1f\", $BASELINE_LINE * 100}")
    BASELINE_BRANCH_PCT=$(awk "BEGIN {printf \"%.1f\", $BASELINE_BRANCH * 100}")
    echo -e "${GREEN}✓${NC} Baseline coverage: ${BASELINE_LINE_PCT}% line, ${BASELINE_BRANCH_PCT}% branch"
else
    echo -e "${YELLOW}⚠${NC} Coverage report not generated (this is expected for empty tests)"
    BASELINE_LINE_PCT="0.0"
    BASELINE_BRANCH_PCT="0.0"
fi
echo ""

# Step 3: Run Orchestrator with Mock AI
echo "──────────────────────────────────────────────────────"
echo "STEP 3: Running orchestrator with Mock AI provider..."
echo "──────────────────────────────────────────────────────"
timeout 60s dotnet run --project src/ReverseCoverage.Orchestrator/ReverseCoverage.Orchestrator.csproj --no-build -- \
    --solution-path "$WORKSPACE_ROOT/ReverseCoverage.sln" \
    --test-project-path "$WORKSPACE_ROOT/src/DemoCalc.Tests/DemoCalc.Tests.csproj" \
    --coverage-threshold 80 \
    --iteration-budget 3 \
    --provider Mock || true

echo -e "${GREEN}✓${NC} Orchestrator run completed"
echo ""

# Step 4: Rebuild tests to include generated ones
echo "──────────────────────────────────────────────────────"
echo "STEP 4: Rebuilding tests with generated code..."
echo "──────────────────────────────────────────────────────"
if [ -d "src/DemoCalc.Tests/Generated" ]; then
    TEST_FILE_COUNT=$(find src/DemoCalc.Tests/Generated -name "*.cs" 2>/dev/null | wc -l)
    echo "Found $TEST_FILE_COUNT generated test files"
    
    dotnet build src/DemoCalc.Tests/DemoCalc.Tests.csproj --configuration Debug --verbosity quiet
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓${NC} Tests rebuilt successfully"
    else
        echo -e "${RED}✗${NC} Test rebuild failed - checking generated code quality"
        echo ""
        echo "Generated test files:"
        find src/DemoCalc.Tests/Generated -name "*.cs" -exec echo "  - {}" \;
    fi
else
    echo -e "${YELLOW}⚠${NC} No generated test files found"
fi
echo ""

# Step 5: Run all tests (including generated)
echo "──────────────────────────────────────────────────────"
echo "STEP 5: Running all tests..."
echo "──────────────────────────────────────────────────────"
dotnet test src/DemoCalc.Tests/DemoCalc.Tests.csproj --nologo --verbosity normal || true
echo ""

# Step 6: Measure improved coverage
echo "──────────────────────────────────────────────────────"
echo "STEP 6: Measuring improved coverage..."
echo "──────────────────────────────────────────────────────"
mkdir -p artifacts/feasibility/improved
dotnet test src/DemoCalc.Tests/DemoCalc.Tests.csproj \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput="../../artifacts/feasibility/improved/" \
    --nologo --verbosity quiet || true

if [ -f "artifacts/feasibility/improved/coverage.cobertura.xml" ]; then
    IMPROVED_LINE=$(grep 'line-rate' artifacts/feasibility/improved/coverage.cobertura.xml | head -1 | sed -n 's/.*line-rate="\([^"]*\)".*/\1/p')
    IMPROVED_BRANCH=$(grep 'branch-rate' artifacts/feasibility/improved/coverage.cobertura.xml | head -1 | sed -n 's/.*branch-rate="\([^"]*\)".*/\1/p')
    IMPROVED_LINE_PCT=$(awk "BEGIN {printf \"%.1f\", $IMPROVED_LINE * 100}")
    IMPROVED_BRANCH_PCT=$(awk "BEGIN {printf \"%.1f\", $IMPROVED_BRANCH * 100}")
    echo -e "${GREEN}✓${NC} Improved coverage: ${IMPROVED_LINE_PCT}% line, ${IMPROVED_BRANCH_PCT}% branch"
    
    # Calculate improvement
    LINE_DELTA=$(awk "BEGIN {printf \"%.1f\", $IMPROVED_LINE_PCT - $BASELINE_LINE_PCT}")
    BRANCH_DELTA=$(awk "BEGIN {printf \"%.1f\", $IMPROVED_BRANCH_PCT - $BASELINE_BRANCH_PCT}")
    
    echo ""
    echo -e "${GREEN}Δ Coverage Improvement:${NC}"
    echo -e "  Line coverage:   +${LINE_DELTA}%"
    echo -e "  Branch coverage: +${BRANCH_DELTA}%"
else
    echo -e "${YELLOW}⚠${NC} Coverage report not generated"
fi
echo ""

# Step 7: Final Summary
echo "═══════════════════════════════════════════════════════"
echo "   FEASIBILITY TEST SUMMARY"
echo "═══════════════════════════════════════════════════════"
echo ""
echo "Baseline Coverage:  ${BASELINE_LINE_PCT}% line, ${BASELINE_BRANCH_PCT}% branch"
echo "Improved Coverage:  ${IMPROVED_LINE_PCT}% line, ${IMPROVED_BRANCH_PCT}% branch"
echo "Improvement:        +${LINE_DELTA}% line, +${BRANCH_DELTA}% branch"
echo ""

if [ -d "src/DemoCalc.Tests/Generated" ]; then
    echo "Generated Files:"
    find src/DemoCalc.Tests/Generated -name "*.cs" -exec basename {} \; | sed 's/^/  - /'
    echo ""
fi

# Determine success
if (( $(echo "$LINE_DELTA > 0" | bc -l) )); then
    echo -e "${GREEN}✓ FEASIBILITY TEST PASSED${NC}"
    echo "  The framework successfully generated tests and improved coverage!"
    EXIT_CODE=0
else
    echo -e "${RED}✗ FEASIBILITY TEST FAILED${NC}"
    echo "  Coverage did not improve. Check the orchestrator output above."
    EXIT_CODE=1
fi

echo "═══════════════════════════════════════════════════════"
echo ""
echo "Artifacts saved to: artifacts/feasibility/"
echo "  - baseline/coverage.cobertura.xml"
echo "  - improved/coverage.cobertura.xml"
echo ""

exit $EXIT_CODE
