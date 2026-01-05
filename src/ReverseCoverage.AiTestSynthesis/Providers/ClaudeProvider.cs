// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis.Providers;

/// <summary>
/// Claude API provider for test generation (Anthropic).
/// </summary>
public class ClaudeProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;
    private readonly HttpClient _httpClient;

    public ClaudeProvider(AiTestSynthesisOptions options)
    {
        _options = options;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.BaseUrl ?? "https://api.anthropic.com/v1/"),
            Timeout = _options.RequestTimeout
        };

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }
    }

    public async Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);

        var requestBody = new
        {
            model = _options.Model ?? "claude-3-5-sonnet-20241022",
            max_tokens = _options.MaxOutputTokens,
            temperature = _options.Temperature,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        try
        {
            if (_options.LogRequestBodies)
            {
                Console.WriteLine($"[ClaudeProvider] Sending request to Claude API...");
            }

            var response = await _httpClient.PostAsJsonAsync("messages", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = Array.Empty<TestCaseSpec>(),
                    Notes = new[] { $"Claude API error: {response.StatusCode}", errorContent }
                };
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (_options.LogResponses)
            {
                Console.WriteLine($"[ClaudeProvider] Received response from Claude");
            }

            // Parse Claude response
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(content);
            
            if (claudeResponse?.Content != null && claudeResponse.Content.Length > 0)
            {
                var generatedText = claudeResponse.Content[0].Text;
                
                // Parse the generated test specifications
                var tests = ParseTestSpecifications(generatedText, request);
                
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = tests.ToArray(),
                    Notes = new[] { $"Claude generated {tests.Count} test specifications" }
                };
            }

            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { "Claude returned empty response" }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ClaudeProvider] Error: {ex.Message}");
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"Error: {ex.Message}" }
            };
        }
    }

    private string BuildPrompt(GenerationRequest request)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("You are an expert C# test engineer. Generate comprehensive unit tests for the following method.");
        sb.AppendLine();
        sb.AppendLine("Method Signature:");
        sb.AppendLine(request.MethodSignature);
        sb.AppendLine();
        sb.AppendLine("Method Source Code:");
        sb.AppendLine(request.MethodSource);
        sb.AppendLine();
        
        if (request.BranchHints.Length > 0)
        {
            sb.AppendLine("Branch Points to Cover:");
            foreach (var hint in request.BranchHints)
            {
                sb.AppendLine($"- {hint.Kind}: {string.Join(", ", hint.SuggestedMutations)}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Requirements:");
        sb.AppendLine($"- Test Framework: {_options.TestFramework}");
        sb.AppendLine($"- Mocking Framework: {_options.Mocking}");
        sb.AppendLine($"- Generate tests that cover ALL branches");
        sb.AppendLine($"- Include edge cases (null, empty, invalid inputs)");
        sb.AppendLine($"- Include happy path tests");
        sb.AppendLine();
        
        sb.AppendLine("Generate test specifications in this format for EACH test:");
        sb.AppendLine("TEST: [descriptive name]");
        sb.AppendLine("ARRANGE: [setup code]");
        sb.AppendLine("ACT: [method call]");
        sb.AppendLine("ASSERT: [expected behavior]");
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("Generate 5-10 comprehensive tests now:");

        return sb.ToString();
    }

    private List<TestCaseSpec> ParseTestSpecifications(string generatedText, GenerationRequest request)
    {
        var tests = new List<TestCaseSpec>();
        
        // Split by test separator
        var testBlocks = generatedText.Split(new[] { "---", "TEST:" }, StringSplitOptions.RemoveEmptyEntries);
        
        int testNum = 1;
        foreach (var block in testBlocks)
        {
            if (string.IsNullOrWhiteSpace(block)) continue;
            
            var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            string testName = $"GeneratedTest_{testNum}";
            string arrangeCode = "";
            string actCode = "";
            string assertCode = "";
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                if (trimmed.StartsWith("TEST:", StringComparison.OrdinalIgnoreCase))
                {
                    testName = trimmed.Substring(5).Trim();
                    testName = SanitizeTestName(testName);
                }
                else if (trimmed.StartsWith("ARRANGE:", StringComparison.OrdinalIgnoreCase))
                {
                    arrangeCode = trimmed.Substring(8).Trim();
                }
                else if (trimmed.StartsWith("ACT:", StringComparison.OrdinalIgnoreCase))
                {
                    actCode = trimmed.Substring(4).Trim();
                }
                else if (trimmed.StartsWith("ASSERT:", StringComparison.OrdinalIgnoreCase))
                {
                    assertCode = trimmed.Substring(7).Trim();
                }
            }
            
            if (!string.IsNullOrEmpty(testName))
            {
                var test = new TestCaseSpec
                {
                    TestName = testName,
                    TargetMethodId = request.TargetMethod.MethodId ?? "Unknown",
                    Arrange = new[] { new StepSpec { Kind = "Code", Text = arrangeCode } },
                    Act = new StepSpec { Kind = "Code", Text = actCode },
                    Assert = new[] { new AssertionSpec { Kind = "Code", Text = assertCode } }
                };
                
                tests.Add(test);
                testNum++;
            }
        }
        
        return tests;
    }

    private string SanitizeTestName(string name)
    {
        // Remove special characters, keep alphanumeric and underscore
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        return string.IsNullOrEmpty(sanitized) ? "GeneratedTest" : sanitized;
    }
}

// Claude API response models
internal class ClaudeResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public ClaudeContent[] Content { get; set; } = Array.Empty<ClaudeContent>();

    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("stop_reason")]
    public string StopReason { get; set; } = "";
}

internal class ClaudeContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";
}
