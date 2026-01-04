// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis.Providers;

/// <summary>
/// Mock AI provider for testing - generates hardcoded test specifications for DemoCalc.
/// </summary>
public class MockAiProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;

    public MockAiProvider(AiTestSynthesisOptions options)
    {
        _options = options;
    }

    public Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Detect if this is the DemoCalc.Evaluate method
        if (request.MethodSignature.Contains("Evaluate") && request.MethodSource.Contains("expressionText"))
        {
            return Task.FromResult(GenerateDemoCalcTests(request));
        }

        // Default: return empty response for unknown methods
        return Task.FromResult(new GenerationResponse
        {
            RequestId = request.RequestId,
            ProposedTests = Array.Empty<TestCaseSpec>(),
            Notes = new[] { $"MockAiProvider: No test generation logic for method {request.MethodSignature}" }
        });
    }

    private GenerationResponse GenerateDemoCalcTests(GenerationRequest request)
    {
        var tests = new List<TestCaseSpec>
        {
            // Test 1: Null input
            new TestCaseSpec
            {
                TestName = "Evaluate_NullInput_ThrowsArgumentException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[]
                {
                    new StepSpec { Kind = "AssignVar", Text = "Set input to null" }
                },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate(null)" },
                Assert = new[]
                {
                    new AssertionSpec { Kind = "Throws", Text = "ArgumentException is thrown" }
                }
            },

            // Test 2: Empty input
            new TestCaseSpec
            {
                TestName = "Evaluate_EmptyInput_ThrowsArgumentException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to empty string" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate(\"\")" },
                Assert = new[] { new AssertionSpec { Kind = "Throws", Text = "ArgumentException is thrown" } }
            },

            // Test 3: Too few tokens
            new TestCaseSpec
            {
                TestName = "Evaluate_TooFewTokens_ThrowsFormatException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '5 +'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate with too few tokens" },
                Assert = new[] { new AssertionSpec { Kind = "Throws", Text = "FormatException is thrown" } }
            },

            // Test 4: Invalid left operand
            new TestCaseSpec
            {
                TestName = "Evaluate_InvalidLeftOperand_ThrowsFormatException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to 'abc + 5'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate with invalid left operand" },
                Assert = new[] { new AssertionSpec { Kind = "Throws", Text = "FormatException is thrown" } }
            },

            // Test 5: Unsupported operator
            new TestCaseSpec
            {
                TestName = "Evaluate_UnsupportedOperator_ThrowsNotSupportedException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '5 % 3'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate with unsupported operator" },
                Assert = new[] { new AssertionSpec { Kind = "Throws", Text = "NotSupportedException is thrown" } }
            },

            // Test 6: Division by zero
            new TestCaseSpec
            {
                TestName = "Evaluate_DivisionByZero_ThrowsDivideByZeroException",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '10 / 0'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate with division by zero" },
                Assert = new[] { new AssertionSpec { Kind = "Throws", Text = "DivideByZeroException is thrown" } }
            },

            // Test 7: Valid addition
            new TestCaseSpec
            {
                TestName = "Evaluate_ValidAddition_ReturnsCorrectResult",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '5 + 3'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result equals 8.0" } }
            },

            // Test 8: Valid subtraction
            new TestCaseSpec
            {
                TestName = "Evaluate_ValidSubtraction_ReturnsCorrectResult",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '10 - 4'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result equals 6.0" } }
            },

            // Test 9: Valid multiplication
            new TestCaseSpec
            {
                TestName = "Evaluate_ValidMultiplication_ReturnsCorrectResult",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '7 * 6'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result equals 42.0" } }
            },

            // Test 10: Valid division
            new TestCaseSpec
            {
                TestName = "Evaluate_ValidDivision_ReturnsCorrectResult",
                TargetMethodId = "DemoCalc|Evaluate",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '20 / 4'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call DemoCalc.Evaluate" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result equals 5.0" } }
            }
        };

        return new GenerationResponse
        {
            RequestId = request.RequestId,
            ProposedTests = tests.ToArray(),
            Notes = new[] { $"MockAiProvider: Generated {tests.Count} test specifications for DemoCalc.Evaluate" }
        };
    }
}
