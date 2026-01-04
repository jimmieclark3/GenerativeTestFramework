// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis.Providers;

/// <summary>
/// OpenAI provider for test generation.
/// </summary>
public class OpenAIProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;
    private readonly HttpClient _httpClient;

    public OpenAIProvider(AiTestSynthesisOptions options)
    {
        _options = options;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_options.BaseUrl ?? "https://api.openai.com/v1/"),
            Timeout = _options.RequestTimeout
        };

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Simplified implementation - would use Structured Outputs API in production
        var prompt = BuildPrompt(request);
        
        var requestBody = new
        {
            model = _options.Model ?? "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a unit-test generator. Output ONLY valid JSON matching the provided schema." },
                new { role = "user", content = prompt }
            },
            temperature = _options.Temperature,
            max_tokens = _options.MaxOutputTokens
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            // TODO: Parse response and validate against schema
            // For now, return stub
            return new GenerationResponse
            {
                RequestId = request.RequestId,
                ProposedTests = Array.Empty<TestCaseSpec>(),
                Notes = new[] { "OpenAIProvider response parsing not yet fully implemented" }
            };
        }
        catch (Exception ex)
        {
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
        sb.AppendLine($"Generate unit tests for method: {request.MethodSignature}");
        sb.AppendLine($"Framework: {_options.TestFramework}");
        sb.AppendLine($"Mocking: {_options.Mocking}");
        sb.AppendLine();
        sb.AppendLine("Method source:");
        sb.AppendLine(request.MethodSource);
        return sb.ToString();
    }
}

