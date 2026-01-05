// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.PluginContracts;

/// <summary>
/// Interface for AI test synthesis providers.
/// </summary>
public interface IAiTestSynthesisProvider
{
    /// <summary>
    /// Generates test specifications for the given request.
    /// </summary>
    Task<GenerationResponse> GenerateAsync(GenerationRequest request, CancellationToken cancellationToken = default);
}

