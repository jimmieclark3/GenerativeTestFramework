// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis.Providers;

/// <summary>
/// Custom HTTP provider for OpenAI-compatible endpoints.
/// </summary>
public class CustomHttpProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;

    public CustomHttpProvider(AiTestSynthesisOptions options)
    {
        _options = options;
    }

    public Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Stub implementation
        return Task.FromResult(new GenerationResponse
        {
            RequestId = request.RequestId,
            ProposedTests = Array.Empty<TestCaseSpec>(),
            Notes = new[] { "CustomHttpProvider not yet implemented" }
        });
    }
}

