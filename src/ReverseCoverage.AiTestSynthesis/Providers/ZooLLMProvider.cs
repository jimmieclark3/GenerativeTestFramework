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
/// ZooLLM API provider for cloud-based LLM test generation.
/// </summary>
public class ZooLLMProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _providerName;
    private readonly string _model;
    private string? _authToken;

    public ZooLLMProvider(AiTestSynthesisOptions options)
    {
        _options = options;
        _baseUrl = _options.BaseUrl ?? "https://api.zoollm.com";
        _providerName = _options.Model; // Provider name from Model field (user must set this)
        _model = "unsloth/gpt-oss-20b-GGUF:gpt-oss-20b-Q4_K_M.gguf"; // Default model
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = _options.RequestTimeout
        };
    }

    public async Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Get provider name if not set (prompt user)
        if (string.IsNullOrEmpty(_providerName))
        {
            Console.Write("ZooLLM Provider Name: ");
            _providerName = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(_providerName))
            {
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = Array.Empty<TestCaseSpec>(),
                    Notes = new[] { "ZooLLM provider name is required" }
                };
            }
        }

        // Ensure we have a valid token
        if (string.IsNullOrEmpty(_authToken))
        {
            _authToken = await LoginAsync(cancellationToken);
            if (string.IsNullOrEmpty(_authToken))
            {
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = Array.Empty<TestCaseSpec>(),
                    Notes = new[] { "ZooLLM authentication failed" }
                };
            }
        }

        var prompt = BuildPrompt(request);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature,
            max_tokens = _options.MaxOutputTokens,
            stream = false
        };

        try
        {
            if (_options.LogRequestBodies)
            {
                Console.WriteLine($"[ZooLLMProvider] Sending request to ZooLLM at {_baseUrl} with provider {_providerName}...");
            }

            var response = await _httpClient.PostAsJsonAsync(
                $"/provider/{_providerName}/v1/chat/completions",
                requestBody,
                cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token expired, re-login
                _authToken = await LoginAsync(cancellationToken);
                if (string.IsNullOrEmpty(_authToken))
                {
                    return new GenerationResponse
                    {
                        RequestId = request.RequestId,
                        ProposedTests = Array.Empty<TestCaseSpec>(),
                        Notes = new[] { "ZooLLM re-authentication failed" }
                    };
                }
                
                // Retry with new token
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                response = await _httpClient.PostAsJsonAsync(
                    $"/provider/{_providerName}/v1/chat/completions",
                    requestBody,
                    cancellationToken);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = Array.Empty<TestCaseSpec>(),
                    Notes = new[] { $"ZooLLM API error: {response.StatusCode}", errorContent }
                };
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (_options.LogResponses)
            {
                Console.WriteLine($"[ZooLLMProvider] Received response from ZooLLM");
            }

            // Parse ZooLLM response (OpenAI-compatible format)
            var zooResponse = JsonSerializer.Deserialize<ZooLLMResponse>(content);
            
            var generatedText = zooResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            
            if (!string.IsNullOrEmpty(generatedText))
            {
                // Parse the generated test specifications
                var tests = ParseTestSpecifications(generatedText, request);
                
                return new GenerationResponse
                {
                    RequestId = request.RequestId,
                    ProposedTests = tests.ToArray(),
                    Notes = new[] { $"ZooLLM ({_model}) generated {tests.Count} test specifications" }
                };
            }

            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { "ZooLLM returned empty response" }
            };
        }
        catch (HttpRequestException ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"ZooLLM connection error: {ex.Message}" }
            };
        }
        catch (TaskCanceledException ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"ZooLLM request timeout: {ex.Message}" }
            };
        }
        catch (Exception ex)
        {
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { $"ZooLLM error: {ex.Message}" }
            };
        }
    }

    private async Task<string?> LoginAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== ZooLLM Authentication Required ===");
        Console.Write("Email: ");
        var email = Console.ReadLine();
        
        Console.Write("Password: ");
        var password = ReadPassword();
        Console.WriteLine();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("ERROR: Email and password are required");
            return null;
        }

        try
        {
            var loginBody = new
            {
                email = email,
                password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/v1/auth/login", loginBody, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"ERROR: Login failed: {errorContent}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content);
            
            if (loginResponse?.Success == true && !string.IsNullOrEmpty(loginResponse.Token))
            {
                _authToken = loginResponse.Token;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                Console.WriteLine("SUCCESS: Authenticated with ZooLLM");
                return _authToken;
            }

            Console.WriteLine("ERROR: Login response missing token");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Login exception: {ex.Message}");
            return null;
        }
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        return password.ToString();
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

    private class LoginResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("user")]
        public JsonElement? User { get; set; }
    }

    private class ZooLLMResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}

