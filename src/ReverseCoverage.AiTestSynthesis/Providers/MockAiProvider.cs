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

        // Detect if this is a MarkdownHelper method
        if (request.MethodSignature.Contains("MarkdownHelper") || request.MethodSource.Contains("namespace MarkdownHelper"))
        {
            return Task.FromResult(GenerateMarkdownHelperTests(request));
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

    private GenerationResponse GenerateMarkdownHelperTests(GenerationRequest request)
    {
        var methodName = ExtractMethodName(request.MethodSignature);
        var tests = new List<TestCaseSpec>();

        // Generate 2-3 tests per method
        if (methodName.Contains("BoldToHtml"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "BoldToHtml_NullInput_ReturnsEmptyString",
                TargetMethodId = "MarkdownHelper|BoldToHtml",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to null" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call BoldToHtml(null)" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result is empty string" } }
            });

            tests.Add(new TestCaseSpec
            {
                TestName = "BoldToHtml_ValidBold_ConvertsToStrong",
                TargetMethodId = "MarkdownHelper|BoldToHtml",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '**bold**'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call BoldToHtml" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result contains <strong>bold</strong>" } }
            });
        }
        else if (methodName.Contains("ItalicToHtml"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "ItalicToHtml_ValidItalic_ConvertsToEm",
                TargetMethodId = "MarkdownHelper|ItalicToHtml",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '*italic*'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call ItalicToHtml" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result contains <em>italic</em>" } }
            });
        }
        else if (methodName.Contains("ExtractHeaders"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "ExtractHeaders_EmptyInput_ReturnsEmptyList",
                TargetMethodId = "MarkdownHelper|ExtractHeaders",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to empty" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call ExtractHeaders" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result is empty list" } }
            });

            tests.Add(new TestCaseSpec
            {
                TestName = "ExtractHeaders_MultipleHeaders_ReturnsAll",
                TargetMethodId = "MarkdownHelper|ExtractHeaders",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input with # Header1 and ## Header2" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call ExtractHeaders" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result contains 2 headers" } }
            });
        }
        else if (methodName.Contains("LinksToHtml"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "LinksToHtml_ValidLink_ConvertsToAnchor",
                TargetMethodId = "MarkdownHelper|LinksToHtml",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to '[text](url)'" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call LinksToHtml" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result contains <a href>" } }
            });
        }
        else if (methodName.Contains("CountCodeBlocks"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "CountCodeBlocks_NoBlocks_ReturnsZero",
                TargetMethodId = "MarkdownHelper|CountCodeBlocks",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input without code blocks" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call CountCodeBlocks" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result is 0" } }
            });

            tests.Add(new TestCaseSpec
            {
                TestName = "CountCodeBlocks_OneBlock_ReturnsOne",
                TargetMethodId = "MarkdownHelper|CountCodeBlocks",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input with one ``` block" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call CountCodeBlocks" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result is 1" } }
            });
        }
        else if (methodName.Contains("ToPlainText"))
        {
            tests.Add(new TestCaseSpec
            {
                TestName = "ToPlainText_EmptyInput_ReturnsEmpty",
                TargetMethodId = "MarkdownHelper|ToPlainText",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input to empty" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call ToPlainText" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result is empty" } }
            });

            tests.Add(new TestCaseSpec
            {
                TestName = "ToPlainText_WithFormatting_StripsAll",
                TargetMethodId = "MarkdownHelper|ToPlainText",
                Arrange = new[] { new StepSpec { Kind = "AssignVar", Text = "Set input with markdown formatting" } },
                Act = new StepSpec { Kind = "CallMethod", Text = "Call ToPlainText" },
                Assert = new[] { new AssertionSpec { Kind = "Equal", Text = "Result has no markdown" } }
            });
        }

        return new GenerationResponse
        {
            RequestId = request.RequestId,
            ProposedTests = tests.ToArray(),
            Notes = new[] { $"MockAiProvider: Generated {tests.Count} test specifications for {methodName}" }
        };
    }

    private string ExtractMethodName(string signature)
    {
        // Simple extraction - just get the method name
        var parts = signature.Split(new[] { ' ', '(', '<' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[parts.Length - 1] : signature;
    }
}

