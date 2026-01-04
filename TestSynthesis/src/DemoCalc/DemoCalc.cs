// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace DemoCalc;

/// <summary>
/// A simple calculator for demonstration purposes.
/// </summary>
public static class DemoCalc
{
    /// <summary>
    /// Evaluates a simple binary expression.
    /// </summary>
    /// <param name="expressionText">Expression in format "operand1 operator operand2" (e.g., "1 + 2")</param>
    /// <returns>The result of the expression</returns>
    /// <exception cref="ArgumentException">Thrown when expression is null, empty, or whitespace</exception>
    /// <exception cref="FormatException">Thrown when expression format is invalid or operands cannot be parsed</exception>
    /// <exception cref="NotSupportedException">Thrown when operator is not supported</exception>
    /// <exception cref="DivideByZeroException">Thrown when dividing by zero</exception>
    public static double Evaluate(string expressionText)
    {
        // B1: null/empty/whitespace check
        if (string.IsNullOrWhiteSpace(expressionText))
        {
            throw new ArgumentException("Expression cannot be null, empty, or whitespace.", nameof(expressionText));
        }

        // Tokenize
        var tokens = expressionText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // B2: expect exactly 3 tokens
        if (tokens.Length != 3)
        {
            throw new FormatException($"Expression must have exactly 3 tokens (operand1 operator operand2), but got {tokens.Length}.");
        }

        // B3: parse left operand
        if (!double.TryParse(tokens[0], out var leftOperand))
        {
            throw new FormatException($"Could not parse left operand: {tokens[0]}");
        }

        var operatorToken = tokens[1];

        // B4: validate operator
        if (operatorToken != "+" && operatorToken != "-" && operatorToken != "*" && operatorToken != "/")
        {
            throw new NotSupportedException($"Operator '{operatorToken}' is not supported. Supported operators: +, -, *, /");
        }

        // B5: parse right operand
        if (!double.TryParse(tokens[2], out var rightOperand))
        {
            throw new FormatException($"Could not parse right operand: {tokens[2]}");
        }

        // B6: division by zero check
        if (operatorToken == "/" && rightOperand == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        // Perform operation
        return operatorToken switch
        {
            "+" => leftOperand + rightOperand,      // B7
            "-" => leftOperand - rightOperand,      // B8
            "*" => leftOperand * rightOperand,      // B9
            "/" => leftOperand / rightOperand,      // B10
            _ => throw new NotSupportedException($"Operator '{operatorToken}' is not supported.")
        };
    }
}

