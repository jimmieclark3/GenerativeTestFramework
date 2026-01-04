// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.ContextCollector.Analyzers;

/// <summary>
/// Extracts branch hints from C# syntax trees.
/// </summary>
public static class BranchHintExtractor
{
    public static BranchHint[] ExtractBranchHints(SyntaxNode methodNode, SemanticModel semanticModel)
    {
        var hints = new List<BranchHint>();

        foreach (var node in methodNode.DescendantNodes())
        {
            switch (node)
            {
                case IfStatementSyntax ifStmt:
                    hints.AddRange(ExtractIfStatementHints(ifStmt, semanticModel));
                    break;

                case SwitchStatementSyntax switchStmt:
                    hints.Add(ExtractSwitchHint(switchStmt, semanticModel));
                    break;

                case ConditionalExpressionSyntax conditional:
                    hints.AddRange(ExtractConditionalHints(conditional, semanticModel));
                    break;

                case BinaryExpressionSyntax binary when IsComparison(binary):
                    hints.Add(ExtractComparisonHint(binary, semanticModel));
                    break;
            }
        }

        return hints.ToArray();
    }

    private static BranchHint[] ExtractIfStatementHints(IfStatementSyntax ifStmt, SemanticModel semanticModel)
    {
        var hints = new List<BranchHint>();
        var condition = ifStmt.Condition;

        // Null checks
        if (IsNullCheck(condition, semanticModel, out var nullCheckOperand))
        {
            hints.Add(new BranchHint
            {
                Kind = "NullCheck",
                Operands = new[] { nullCheckOperand },
                SuggestedMutations = new[] { "null", "non-null value" }
            });
        }

        // Length checks
        if (IsLengthCheck(condition, semanticModel, out var lengthOperand))
        {
            hints.Add(new BranchHint
            {
                Kind = "LengthCheck",
                Operands = new[] { lengthOperand },
                SuggestedMutations = new[] { "empty", "non-empty" }
            });
        }

        // Comparisons
        if (condition is BinaryExpressionSyntax binary && IsComparison(binary))
        {
            hints.Add(ExtractComparisonHint(binary, semanticModel));
        }

        return hints.ToArray();
    }

    private static BranchHint ExtractSwitchHint(SwitchStatementSyntax switchStmt, SemanticModel semanticModel)
    {
        var expression = switchStmt.Expression.ToString();
        var cases = switchStmt.Sections.SelectMany(s => s.Labels.OfType<CaseSwitchLabelSyntax>())
            .Select(l => l.Value.ToString())
            .ToArray();

        return new BranchHint
        {
            Kind = "Switch",
            Operands = new[] { expression },
            SuggestedMutations = cases
        };
    }

    private static BranchHint[] ExtractConditionalHints(ConditionalExpressionSyntax conditional, SemanticModel semanticModel)
    {
        var hints = new List<BranchHint>();

        if (IsNullCheck(conditional.Condition, semanticModel, out var operand))
        {
            hints.Add(new BranchHint
            {
                Kind = "NullCheck",
                Operands = new[] { operand },
                SuggestedMutations = new[] { "null", "non-null value" }
            });
        }

        return hints.ToArray();
    }

    private static BranchHint ExtractComparisonHint(BinaryExpressionSyntax binary, SemanticModel semanticModel)
    {
        var left = binary.Left.ToString();
        var right = binary.Right.ToString();
        var operatorKind = binary.OperatorToken.Kind();

        var suggestedMutations = operatorKind switch
        {
            SyntaxKind.EqualsEqualsToken => new[] { "different value", "same value" },
            SyntaxKind.ExclamationEqualsToken => new[] { "same value", "different value" },
            SyntaxKind.GreaterThanToken => new[] { "smaller value", "larger value" },
            SyntaxKind.LessThanToken => new[] { "larger value", "smaller value" },
            SyntaxKind.GreaterThanEqualsToken => new[] { "smaller value", "equal or larger value" },
            SyntaxKind.LessThanEqualsToken => new[] { "larger value", "equal or smaller value" },
            _ => Array.Empty<string>()
        };

        return new BranchHint
        {
            Kind = "Compare",
            Operands = new[] { left, right },
            SuggestedMutations = suggestedMutations
        };
    }

    private static bool IsNullCheck(ExpressionSyntax expression, SemanticModel semanticModel, out string operand)
    {
        operand = string.Empty;

        if (expression is BinaryExpressionSyntax binary)
        {
            if (binary.OperatorToken.Kind() == SyntaxKind.EqualsEqualsToken ||
                binary.OperatorToken.Kind() == SyntaxKind.ExclamationEqualsToken)
            {
                if (binary.Right is LiteralExpressionSyntax literal && literal.Token.IsKind(SyntaxKind.NullKeyword))
                {
                    operand = binary.Left.ToString();
                    return true;
                }
                if (binary.Left is LiteralExpressionSyntax leftLiteral && leftLiteral.Token.IsKind(SyntaxKind.NullKeyword))
                {
                    operand = binary.Right.ToString();
                    return true;
                }
            }
        }

        if (expression is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            operand = conditionalAccess.Expression.ToString();
            return true;
        }

        return false;
    }

    private static bool IsLengthCheck(ExpressionSyntax expression, SemanticModel semanticModel, out string operand)
    {
        operand = string.Empty;

        if (expression is BinaryExpressionSyntax binary)
        {
            var memberAccess = binary.Left as MemberAccessExpressionSyntax ??
                              binary.Right as MemberAccessExpressionSyntax;

            if (memberAccess?.Name.Identifier.ValueText == "Length")
            {
                operand = memberAccess.Expression.ToString();
                return true;
            }
        }

        return false;
    }

    private static bool IsComparison(BinaryExpressionSyntax binary)
    {
        return binary.OperatorToken.Kind() switch
        {
            SyntaxKind.EqualsEqualsToken => true,
            SyntaxKind.ExclamationEqualsToken => true,
            SyntaxKind.GreaterThanToken => true,
            SyntaxKind.LessThanToken => true,
            SyntaxKind.GreaterThanEqualsToken => true,
            SyntaxKind.LessThanEqualsToken => true,
            _ => false
        };
    }

    private static bool IsComparisonOperator(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.EqualsEqualsToken) ||
               token.IsKind(SyntaxKind.ExclamationEqualsToken) ||
               token.IsKind(SyntaxKind.GreaterThanToken) ||
               token.IsKind(SyntaxKind.LessThanToken) ||
               token.IsKind(SyntaxKind.GreaterThanEqualsToken) ||
               token.IsKind(SyntaxKind.LessThanEqualsToken);
    }
}

