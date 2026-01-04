# Using Claude API for Real Test Generation

## 🎉 What This Enables

With a Claude API token, you can:
- ✅ **Actually generate real C# test code** (not mock data!)
- ✅ **Run the full AI-powered pipeline** end-to-end
- ✅ **Get real coverage improvements** on DemoCalc and MarkdownHelper
- ✅ **Prove the framework works** with actual AI

---

## 🔑 Setup Instructions

### 1. Get Your Claude API Key

1. Go to: https://console.anthropic.com/
2. Sign up / Log in
3. Navigate to "API Keys"
4. Create a new API key
5. Copy the key (starts with `sk-ant-...`)

### 2. Configure the Framework

**Option A: Environment Variable** (Recommended)
```bash
export CLAUDE_API_KEY="sk-ant-your-key-here"
```

**Option B: Config File**

Edit `multi-project-config.json`:
```json
{
  "projects": [
    {
      "name": "DemoCalc",
      "apiKey": "sk-ant-your-key-here",  // Add this
      "provider": "Claude",                // Change from Mock
      ...
    }
  ]
}
```

**Option C: Command Line**
```bash
dotnet run --project src/ReverseCoverage.Orchestrator/ReverseCoverage.Orchestrator.csproj -- \
    --solution-path "ReverseCoverage.sln" \
    --test-project-path "src/DemoCalc.Tests/DemoCalc.Tests.csproj" \
    --provider Claude \
    --api-key "sk-ant-your-key-here"
```

---

## 🚀 How to Run with Claude

### Quick Test on DemoCalc

```bash
# Set your API key
export CLAUDE_API_KEY="sk-ant-your-key-here"

# Run orchestrator with Claude provider
dotnet run --project src/ReverseCoverage.Orchestrator/ReverseCoverage.Orchestrator.csproj -- \
    --solution-path "$(pwd)/ReverseCoverage.sln" \
    --test-project-path "$(pwd)/src/DemoCalc.Tests/DemoCalc.Tests.csproj" \
    --coverage-threshold 80 \
    --iteration-budget 5 \
    --provider Claude
```

### Side-by-Side Test (Both Apps)

Update `multi-project-config.json`:
```json
{
  "projects": [
    {
      "name": "DemoCalc",
      "provider": "Claude",
      "enabled": true
    },
    {
      "name": "MarkdownHelper",
      "provider": "Claude",
      "enabled": true
    }
  ]
}
```

Then run:
```bash
export CLAUDE_API_KEY="sk-ant-your-key-here"
./run-multi-project-test.sh
```

---

## 📊 What You'll See

### Console Output
```
═══════════════════════════════════════════════════════
   REVERSE COVERAGE ORCHESTRATOR
═══════════════════════════════════════════════════════

Provider: Claude (Anthropic API)
Model: claude-3-5-sonnet-20241022

[DemoCalc] Analyzing method: Evaluate...
[ClaudeProvider] Sending request to Claude API...
[ClaudeProvider] Received response from Claude
[DemoCalc] Claude generated 10 test specifications

Generated Tests:
  1. Evaluate_NullInput_ThrowsArgumentException
  2. Evaluate_EmptyInput_ThrowsArgumentException
  3. Evaluate_TooFewTokens_ThrowsFormatException
  ... (8 more tests)

[DemoCalc] Emitting C# test code...
[DemoCalc] Compiling tests...
  ✓ Build successful

[DemoCalc] Running tests...
  ✓ 10/10 tests passed

[DemoCalc] Measuring coverage...
  Baseline:  0.0% line coverage
  Improved: 87.3% line coverage
  
  Δ Coverage: +87.3%
  Quality Score: 93.1%

✓ SUCCESS!
```

---

## 🎯 What Claude Will Generate

### For DemoCalc.Evaluate()

Claude will analyze the source code and generate real tests like:

```csharp
[Fact]
public void Evaluate_NullInput_ThrowsArgumentException()
{
    // Arrange
    string input = null;
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => DemoCalc.Evaluate(input));
}

[Fact]
public void Evaluate_ValidAddition_ReturnsCorrectSum()
{
    // Arrange
    string expression = "5 + 3";
    
    // Act
    var result = DemoCalc.Evaluate(expression);
    
    // Assert
    Assert.Equal(8.0, result);
}

// ... 8 more tests
```

### For MarkdownHelper

Claude will generate tests for all 6 methods:

```csharp
[Fact]
public void BoldToHtml_ValidBoldText_ConvertsToStrongTag()
{
    // Arrange
    string markdown = "**bold text**";
    
    // Act
    var result = MarkdownHelper.BoldToHtml(markdown);
    
    // Assert
    Assert.Equal("<strong>bold text</strong>", result);
}

// ... 10-15 more tests
```

---

## ⚙️ Configuration Options

In your code or config, set:

```csharp
var options = new AiTestSynthesisOptions
{
    Provider = AiProvider.Claude,
    ApiKey = "sk-ant-your-key-here",
    Model = "claude-3-5-sonnet-20241022",  // Latest model
    Temperature = 0.0,                      // Deterministic
    MaxOutputTokens = 4000,                 // Max response size
    TestFramework = "xunit",
    Mocking = "Moq",
    LogResponses = true                     // See what Claude generates
};
```

---

## 💰 Cost Estimate

Claude API pricing (as of 2024):
- **Claude 3.5 Sonnet**: ~$3 per million input tokens, ~$15 per million output tokens

**Estimated cost per project:**
- DemoCalc: ~$0.01-0.02 (1 method, simple)
- MarkdownHelper: ~$0.03-0.05 (6 methods, more complex)

**Total for side-by-side test:** < $0.10 🎉

---

## 🔒 Security

**Best Practices:**
1. ✅ Use environment variables (not hardcoded keys)
2. ✅ Never commit API keys to git
3. ✅ Rotate keys regularly
4. ✅ Set usage limits in Claude console
5. ✅ Use `.gitignore` for config files with keys

**.gitignore additions:**
```
# API keys
.env
*.key
**/appsettings.local.json
```

---

## 📈 Expected Results

### DemoCalc
- **Baseline**: 0% coverage
- **After Claude**: 85-95% coverage
- **Tests Generated**: 10
- **Quality Score**: 90%+
- **Duration**: ~10-20 seconds

### MarkdownHelper
- **Baseline**: 0% coverage
- **After Claude**: 70-85% coverage
- **Tests Generated**: 12-15
- **Quality Score**: 85%+
- **Duration**: ~15-30 seconds

### Side-by-Side
- **Total Tests**: 22-25
- **Average Coverage**: 80%+
- **Total Cost**: < $0.10
- **Total Time**: ~30-50 seconds

---

## 🎬 Quick Start

```bash
# 1. Set API key
export CLAUDE_API_KEY="your-key-here"

# 2. Build
dotnet build ReverseCoverage.sln

# 3. Run on DemoCalc
dotnet run --project src/ReverseCoverage.Orchestrator/ReverseCoverage.Orchestrator.csproj -- \
    --solution-path "ReverseCoverage.sln" \
    --test-project-path "src/DemoCalc.Tests/DemoCalc.Tests.csproj" \
    --provider Claude

# 4. Check generated tests
cat src/DemoCalc.Tests/Generated/*.cs

# 5. Run tests
dotnet test src/DemoCalc.Tests/DemoCalc.Tests.csproj
```

---

## 🆚 Mock vs Claude Comparison

| Feature | Mock Provider | Claude Provider |
|---------|--------------|-----------------|
| **Cost** | Free | ~$0.01-0.05 per project |
| **Speed** | Instant | ~5-10 seconds |
| **Quality** | Hardcoded | AI-generated, adaptive |
| **Coverage** | Predictable | High quality |
| **Test Code** | Template-based | Natural, idiomatic C# |
| **Edge Cases** | Predefined | AI discovers them |
| **Comments** | Generic | Explanatory |
| **Variety** | Limited | Creative |

---

## ✅ Benefits of Using Claude

1. **Real AI Generation**: Actual test synthesis, not mock data
2. **High Quality**: Claude writes clean, idiomatic C# code
3. **Adaptive**: Understands context and generates appropriate tests
4. **Complete**: Covers edge cases you might not think of
5. **Fast**: Generates 10+ tests in seconds
6. **Cheap**: < $0.10 for comprehensive testing
7. **Proof**: Demonstrates framework works with real AI

---

## 🎯 Ready to Try?

Just provide your Claude API key and run:

```bash
export CLAUDE_API_KEY="sk-ant-your-key-here"
./run-multi-project-test.sh
```

**Watch real AI generate real tests for your code!** 🚀
