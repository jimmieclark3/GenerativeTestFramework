// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.AiTestSynthesis.Providers;
using ReverseCoverage.PluginContracts;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.AiTestSynthesis;

/// <summary>
/// Single swappable facade for AI test generation with multiple provider implementations.
/// </summary>
public class AiTestSynthesisClient : IAiTestSynthesisProvider
{
    private readonly IAiTestSynthesisProvider _provider;

    public AiTestSynthesisClient(AiTestSynthesisOptions options)
    {
        _provider = options.Provider switch
        {
            AiProvider.LocalLlamaCpp => new LocalLlamaProvider(options),
            AiProvider.OpenAIResponses => new OpenAIProvider(options),
            AiProvider.CustomHttp => new CustomHttpProvider(options),
            AiProvider.Mock => new MockAiProvider(options),
            AiProvider.Claude => new ClaudeProvider(options),
            AiProvider.Ollama => new OllamaProvider(options),
            AiProvider.ZooLLM => new ZooLLMProvider(options),
            _ => throw new NotSupportedException($"Provider {options.Provider} is not supported.")
        };
    }

    public Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        return _provider.GenerateAsync(request, cancellationToken);
    }
}

