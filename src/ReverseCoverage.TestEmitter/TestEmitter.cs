// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.TestEmitter;

/// <summary>
/// Converts TestCaseSpec templates into compilable C# test files using Roslyn.
/// </summary>
public class TestEmitter
{
    private readonly string _testFramework;
    private readonly string _mockingFramework;

    public TestEmitter(string testFramework = "xunit", string mockingFramework = "Moq")
    {
        _testFramework = testFramework;
        _mockingFramework = mockingFramework;
    }

    /// <summary>
    /// Emits test files from a generation response.
    /// </summary>
    public async Task<string[]> EmitTestsAsync(
        GenerationResponse response,
        string testProjectPath,
        CancellationToken cancellationToken = default)
    {
        var generatedFiles = new List<string>();
        var generatedDir = Path.Combine(testProjectPath, "Generated");
        Directory.CreateDirectory(generatedDir);

        // Group tests by target type
        var testsByType = response.ProposedTests
            .GroupBy(t => t.TargetMethodId.Split('|').FirstOrDefault() ?? "Unknown")
            .ToList();

        foreach (var group in testsByType)
        {
            var typeName = SanitizeTypeName(group.Key);
            var className = $"{typeName}Tests";
            var fileName = Path.Combine(generatedDir, $"{className}.cs");

            var testCode = GenerateTestClass(className, group.ToArray());
            await File.WriteAllTextAsync(fileName, testCode, cancellationToken);
            generatedFiles.Add(fileName);
        }

        return generatedFiles.ToArray();
    }

    private string GenerateTestClass(string className, TestCaseSpec[] testCases)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("// AUTO-GENERATED - do not hand-edit");
        sb.AppendLine("using System;");
        if (_testFramework == "xunit")
        {
            sb.AppendLine("using Xunit;");
        }
        else if (_testFramework == "nunit")
        {
            sb.AppendLine("using NUnit.Framework;");
        }
        else if (_testFramework == "mstest")
        {
            sb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
        }

        if (_mockingFramework == "Moq")
        {
            sb.AppendLine("using Moq;");
        }
        else if (_mockingFramework == "NSubstitute")
        {
            sb.AppendLine("using NSubstitute;");
        }

        sb.AppendLine();
        sb.AppendLine($"namespace Generated.Tests");
        sb.AppendLine("{");
        sb.AppendLine($"    public class {className}");
        sb.AppendLine("    {");

        foreach (var testCase in testCases)
        {
            var methodName = SanitizeMethodName(testCase.TestName);
            var attribute = _testFramework switch
            {
                "xunit" => "[Fact]",
                "nunit" => "[Test]",
                "mstest" => "[TestMethod]",
                _ => "[Fact]"
            };

            sb.AppendLine($"        {attribute}");
            sb.AppendLine($"        public void {methodName}()");
            sb.AppendLine("        {");
            
            // Arrange
            foreach (var step in testCase.Arrange)
            {
                sb.AppendLine($"            // Arrange: {step.Text}");
                sb.AppendLine(GenerateArrangeCode(step));
            }

            // Act
            sb.AppendLine($"            // Act: {testCase.Act.Text}");
            sb.AppendLine(GenerateActCode(testCase.Act));

            // Assert
            foreach (var assertion in testCase.Assert)
            {
                sb.AppendLine($"            // Assert: {assertion.Text}");
                sb.AppendLine(GenerateAssertCode(assertion));
            }

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateArrangeCode(StepSpec step)
    {
        // Simplified implementation - in production, this would parse step.Text more intelligently
        return $"            // TODO: Implement arrange step: {step.Kind} - {step.Text}";
    }

    private string GenerateActCode(StepSpec act)
    {
        // Simplified implementation
        return $"            // TODO: Implement act step: {act.Text}";
    }

    private string GenerateAssertCode(AssertionSpec assertion)
    {
        return assertion.Kind switch
        {
            "Throws" => $"            Assert.Throws<Exception>(() => {{ /* {assertion.Text} */ }});",
            "Equal" => $"            Assert.Equal(/* expected */, /* actual */); // {assertion.Text}",
            "NotNull" => $"            Assert.NotNull(/* value */); // {assertion.Text}",
            "True" => $"            Assert.True(/* condition */); // {assertion.Text}",
            "False" => $"            Assert.False(/* condition */); // {assertion.Text}",
            _ => $"            // Assert: {assertion.Kind} - {assertion.Text}"
        };
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

