#!/usr/bin/env python3
"""
Test Claude on Shouldly's Is.InRange method
Budget: ~$0.02
"""

import os
import json
import urllib.request

API_KEY = os.environ.get("CLAUDE_API_KEY", "")
if not API_KEY:
    print("Error: CLAUDE_API_KEY environment variable not set")
    print("Usage: export CLAUDE_API_KEY='your-key' && python3 test_shouldly.py")
    exit(1)

print("═══════════════════════════════════════════════════════")
print("   TESTING CLAUDE ON REAL OPEN-SOURCE PROJECT")
print("═══════════════════════════════════════════════════════")
print()
print("Project: Shouldly (assertion library)")
print("Target: Is.InRange<T>() method")
print()

# Read the Shouldly Is.cs file
with open('/tmp/shouldly/src/Shouldly/Internals/Is.cs', 'r') as f:
    source_code = f.read()

prompt = f"""You are an expert C# test engineer. Generate comprehensive xUnit tests for this method from the Shouldly library:

```csharp
public static bool InRange<T>(T comparable, T? from, T? to)
    where T : IComparable<T> =>
    comparable.CompareTo(from) >= 0 &&
    comparable.CompareTo(to) <= 0;
```

Context: This is a helper method that checks if a value is within a range (inclusive).

Requirements:
1. Generate 6-8 xUnit test methods
2. Test with different types (int, string, DateTime, etc.)
3. Test edge cases (boundary values, null handling if applicable)
4. Use [Fact] or [Theory] with [InlineData]
5. Class name: IsInRangeTests
6. Namespace: Shouldly.Tests
7. Target: Is.InRange<T>()

Write compilable C# test code only. Include proper using statements."""

request_data = {
    "model": "claude-sonnet-4-5-20250929",
    "max_tokens": 1500,
    "temperature": 0,
    "messages": [{"role": "user", "content": prompt}]
}

print("Calling Claude API...")
print()

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
            
            # Extract C# code from markdown
            if '```csharp' in generated_text:
                code_start = generated_text.find('```csharp') + 9
                code_end = generated_text.find('```', code_start)
                generated_code = generated_text[code_start:code_end].strip()
            elif '```' in generated_text:
                code_start = generated_text.find('```') + 3
                code_end = generated_text.find('```', code_start)
                generated_code = generated_text[code_start:code_end].strip()
            else:
                generated_code = generated_text
            
            # Print stats
            input_tokens = result.get('usage', {}).get('input_tokens', 0)
            output_tokens = result.get('usage', {}).get('output_tokens', 0)
            cost = input_tokens * 0.000003 + output_tokens * 0.000015
            
            print("✓ Claude generated tests successfully!")
            print(f"  Input tokens: {input_tokens}")
            print(f"  Output tokens: {output_tokens}")
            print(f"  Cost: ${cost:.4f}")
            print()
            print("═══════════════════════════════════════════════════════")
            print("GENERATED TESTS:")
            print("═══════════════════════════════════════════════════════")
            print()
            print(generated_code)
            print()
            print("═══════════════════════════════════════════════════════")
            print(f"SUCCESS: Tests generated for real open-source project!")
            print(f"Total cost: ${cost:.4f}")
            print("═══════════════════════════════════════════════════════")
            
        else:
            print("✗ No content in response")
            exit(1)
            
except Exception as e:
    print(f"✗ Error: {e}")
    exit(1)
