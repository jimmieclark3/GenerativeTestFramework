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
/// Ollama provider for local LLM test generation.
/// </summary>
public class OllamaProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;

    public OllamaProvider(AiTestSynthesisOptions options)
    {
        _options = options;
        _baseUrl = _options.BaseUrl ?? "http://localhost:11434";
        _model = _options.Model ?? "gpt-oss:20b"; // Default to OpenAI's open-source model
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = _options.RequestTimeout
        };
    }

    public async Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(request);

        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = false,
            options = new
            {
                temperature = _options.Temperature,
                top_p = _options.TopP,
                num_predict = _options.MaxOutputTokens,
                seed = _options.Seed
            }
        };

        try
        {
            if (_options.LogRequestBodies)
            {
                Console.WriteLine($"[OllamaProvider] Sending request to Ollama at {_baseUrl} with model {_model}...");
            }

            var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = Array.Empty<TestCaseSpec>(),
                    Notes = new[] { $"Ollama API error: {response.StatusCode}", errorContent }
                };
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (_options.LogResponses)
            {
                Console.WriteLine($"[OllamaProvider] Received response from Ollama");
            }

            // Parse Ollama response
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(content);
            
            // gpt-oss:20b outputs to "thinking" field, fallback to "response" field
            var generatedText = ollamaResponse?.Thinking ?? ollamaResponse?.Response;
            
            if (!string.IsNullOrEmpty(generatedText))
            {
                // Parse the generated test specifications
                var tests = ParseTestSpecifications(generatedText, request);
                
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = tests.ToArray(),
                    Notes = new[] { $"Ollama ({_model}) generated {tests.Count} test specifications" }
                };
            }

            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { "Ollama returned empty response" }
            };
        }
        catch (HttpRequestException ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"Ollama connection error: {ex.Message}. Make sure Ollama is running at {_baseUrl}" }
            };
        }
        catch (TaskCanceledException ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"Ollama request timeout: {ex.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"Ollama error: {ex.Message}" }
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

    private class OllamaResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
