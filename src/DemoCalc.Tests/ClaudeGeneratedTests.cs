using System;
using Xunit;

namespace DemoCalc.Tests
{
    public class ClaudeGeneratedTests
    {
        [Fact]
        public void Evaluate_NullExpression_ThrowsArgumentException()
        {
            var exception = Assert.Throws<ArgumentException>(() => DemoCalc.Evaluate(null));
            Assert.Contains("Expression cannot be null, empty, or whitespace.", exception.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void Evaluate_EmptyOrWhitespaceExpression_ThrowsArgumentException(string expression)
        {
            var exception = Assert.Throws<ArgumentException>(() => DemoCalc.Evaluate(expression));
            Assert.Contains("Expression cannot be null, empty, or whitespace.", exception.Message);
        }

        [Theory]
        [InlineData("5 +")]
        [InlineData("5")]
        [InlineData("5 + 3 + 2")]
        [InlineData("5 + 3 + 2 + 1")]
        public void Evaluate_InvalidTokenCount_ThrowsFormatException(string expression)
        {
            var exception = Assert.Throws<FormatException>(() => DemoCalc.Evaluate(expression));
            Assert.Contains("Expression must have exactly 3 tokens", exception.Message);
        }

        [Theory]
        [InlineData("abc + 5")]
        [InlineData("invalid + 5")]
        [InlineData("x + 10")]
        public void Evaluate_InvalidLeftOperand_ThrowsFormatException(string expression)
        {
            var exception = Assert.Throws<FormatException>(() => DemoCalc.Evaluate(expression));
            Assert.Contains("Could not parse left operand", exception.Message);
        }

        [Theory]
        [InlineData("5 % 3")]
        [InlineData("5 ^ 2")]
        [InlineData("5 & 3")]
        [InlineData("5 add 3")]
        public void Evaluate_UnsupportedOperator_ThrowsNotSupportedException(string expression)
        {
            var exception = Assert.Throws<NotSupportedException>(() => DemoCalc.Evaluate(expression));
            Assert.Contains("is not supported", exception.Message);
        }

        [Theory]
        [InlineData("5 + abc")]
        [InlineData("10 - invalid")]
        [InlineData("3 * xyz")]
        public void Evaluate_InvalidRightOperand_ThrowsFormatException(string expression)
        {
            var exception = Assert.Throws<FormatException>(() => DemoCalc.Evaluate(expression));
            Assert.Contains("Could not parse right operand", exception.Message);
        }

        [Theory]
        [InlineData("10 / 0")]
        [InlineData("5.5 / 0.0")]
        [InlineData("-8 / 0")]
        public void Evaluate_DivisionByZero_ThrowsDivideByZeroException(string expression)
        {
            Assert.Throws<DivideByZeroException>(() => DemoCalc.Evaluate(expression));
        }

        [Theory]
        [InlineData("5 + 3", 8.0)]
        [InlineData("10 + 20", 30.0)]
        [InlineData("5.5 + 2.5", 8.0)]
        [InlineData("-5 + 10", 5.0)]
        public void Evaluate_Addition_ReturnsCorrectResult(string expression, double expected)
        {
            var result = DemoCalc.Evaluate(expression);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("10 - 3", 7.0)]
        [InlineData("5 - 10", -5.0)]
        [InlineData("100.5 - 0.5", 100.0)]
        public void Evaluate_Subtraction_ReturnsCorrectResult(string expression, double expected)
        {
            var result = DemoCalc.Evaluate(expression);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("5 * 3", 15.0)]
        [InlineData("10 * 0", 0.0)]
        [InlineData("2.5 * 4", 10.0)]
        [InlineData("-3 * 5", -15.0)]
        public void Evaluate_Multiplication_ReturnsCorrectResult(string expression, double expected)
        {
            var result = DemoCalc.Evaluate(expression);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("10 / 2", 5.0)]
        [InlineData("15 / 3", 5.0)]
        [InlineData("7 / 2", 3.5)]
        [InlineData("-10 / 5", -2.0)]
        public void Evaluate_Division_ReturnsCorrectResult(string expression, double expected)
        {
            var result = DemoCalc.Evaluate(expression);
            Assert.Equal(expected, result);
        }
    }
}