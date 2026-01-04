#!/usr/bin/env python3
"""
Quick Claude API demo - generates tests for DemoCalc
Budget-friendly: Single API call, ~$0.02 cost
"""

import os
import json
import urllib.request
import urllib.error

API_KEY = os.environ.get("CLAUDE_API_KEY", "")
if not API_KEY:
    print("Error: CLAUDE_API_KEY environment variable not set")
    print("Usage: export CLAUDE_API_KEY='your-key' && python3 demo_claude.py")
    exit(1)

print("═══════════════════════════════════════════════════════")
print("   CLAUDE API TEST GENERATION DEMO")
print("═══════════════════════════════════════════════════════")
print()
print("Target: DemoCalc.Evaluate() method")
print("Budget: ~$0.02 for this test")
print()
print("Calling Claude API...")
print()

# Build the prompt
prompt = """You are an expert C# test engineer. Generate comprehensive xUnit tests for this C# method:

```csharp
public static double Evaluate(string expressionText)
{
    // B1: null/empty/whitespace check
    if (string.IsNullOrWhiteSpace(expressionText))
    {
        throw new ArgumentException("Expression cannot be null, empty, or whitespace.", nameof(expressionText));
    }

    var tokens = expressionText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

    // B2: expect exactly 3 tokens
    if (tokens.Length != 3)
    {
        throw new FormatException($"Expression must have exactly 3 tokens, but got {tokens.Length}.");
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
        throw new NotSupportedException($"Operator '{operatorToken}' is not supported.");
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

    // Perform operation (B7-B10)
    return operatorToken switch
    {
        "+" => leftOperand + rightOperand,
        "-" => leftOperand - rightOperand,
        "*" => leftOperand * rightOperand,
        "/" => leftOperand / rightOperand,
        _ => throw new NotSupportedException()
    };
}
```

Generate 5-8 xUnit test methods that cover all 10 branch points (B1-B10). Write complete, runnable C# test code with [Fact] attributes. Include proper using statements."""

# Prepare request
request_data = {
    "model": "claude-sonnet-4-5-20250929",
    "max_tokens": 2000,
    "temperature": 0,
    "messages": [
        {
            "role": "user",
            "content": prompt
        }
    ]
}

# Make API call
try:
    req = urllib.request.Request(
        "https://api.anthropic.com/v1/messages",
        data=json.dumps(request_data).encode('utf-8'),
        headers={
            "content-type": "application/json",
            "x-api-key": API_KEY,
            "anthropic-version": "2023-06-01"
        }
    )
    
    with urllib.request.urlopen(req, timeout=30) as response:
        result = json.loads(response.read().decode('utf-8'))
        
        if 'content' in result and len(result['content']) > 0:
            generated_text = result['content'][0]['text']
            
            print("✓ Claude generated tests successfully!")
            print()
            print("═══════════════════════════════════════════════════════")
            print("GENERATED TEST CODE:")
            print("═══════════════════════════════════════════════════════")
            print()
            print(generated_text)
            print()
            print("═══════════════════════════════════════════════════════")
            print(f"Usage:")
            print(f"  Input tokens: {result.get('usage', {}).get('input_tokens', 0)}")
            print(f"  Output tokens: {result.get('usage', {}).get('output_tokens', 0)}")
            
            # Rough cost estimate
            input_cost = result.get('usage', {}).get('input_tokens', 0) * 0.000003
            output_cost = result.get('usage', {}).get('output_tokens', 0) * 0.000015
            total_cost = input_cost + output_cost
            print(f"  Estimated cost: ${total_cost:.4f}")
            print("═══════════════════════════════════════════════════════")
            
        else:
            print("✗ No content in response")
            print(json.dumps(result, indent=2))
            
except urllib.error.HTTPError as e:
    print(f"✗ HTTP Error: {e.code}")
    print(e.read().decode('utf-8'))
except Exception as e:
    print(f"✗ Error: {e}")
