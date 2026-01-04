// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis.Providers;

/// <summary>
/// Local LLama provider (stub implementation - requires LLamaSharp with proper license verification).
/// </summary>
public class LocalLlamaProvider : IAiTestSynthesisProvider
{
    private readonly AiTestSynthesisOptions _options;

    public LocalLlamaProvider(AiTestSynthesisOptions options)
    {
        _options = options;
        // TODO: Verify LLamaSharp license before use
        // TODO: Load GGUF model from options.ModelPath
    }

    public Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        // Stub implementation - returns empty response
        // TODO: Implement with LLamaSharp after license verification
        return Task.FromResult(new GenerationResponse
        {
            RequestId = request.RequestId,
            ProposedTests = Array.Empty<TestCaseSpec>(),
            Notes = new[] { "LocalLlamaProvider not yet implemented - requires LLamaSharp license verification" }
        });
    }
}

