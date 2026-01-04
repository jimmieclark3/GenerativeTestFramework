// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace ReverseCoverage.PluginContracts;

/// <summary>
/// Options for configuring AI test synthesis.
/// </summary>
public class AiTestSynthesisOptions
{
    /// <summary>
    /// Provider selection: LocalLlamaCpp, OpenAIResponses, or CustomHttp.
    /// </summary>
    public AiProvider Provider { get; set; } = AiProvider.LocalLlamaCpp;

    // Determinism controls
    public double Temperature { get; set; } = 0.0;
    public double TopP { get; set; } = 1.0;
    public int? Seed { get; set; }
    public int MaxOutputTokens { get; set; } = 2000;

    // Timeouts and retries
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    public int BackoffMs { get; set; } = 1000;

    // Schema enforcement
    public bool RequireJsonSchemaOutput { get; set; } = true;
    public string? SchemaName { get; set; }
    public string? SchemaJson { get; set; }

    // Logging
    public bool LogRequestBodies { get; set; } = false;
    public bool LogResponses { get; set; } = true;
    public bool RedactSecrets { get; set; } = true;

    // Test architecture knobs
    public string TestFramework { get; set; } = "xunit"; // xunit, nunit, mstest
    public string OutputLayout { get; set; } = "per-type"; // per-type, per-method
    public string NamingConvention { get; set; } = "{Type}_{Method}_Should_{Behavior}";
    public bool UseTheoryForParameterized { get; set; } = true;
    public string AssertionStyle { get; set; } = "BuiltIn"; // FluentAssertions, BuiltIn
    public string Mocking { get; set; } = "Moq"; // Moq, NSubstitute, None
    public bool NullableContext { get; set; } = true;
    public string SnapshotStrategy { get; set; } = "Off"; // Off, ReturnValue, ExceptionOnly, DTOJson

    // Provider-specific settings
    public string? ModelPath { get; set; } // For LocalLlamaCpp
    public string? ApiKey { get; set; } // For OpenAI/CustomHttp
    public string? BaseUrl { get; set; } // For CustomHttp
    public string? Model { get; set; } // For OpenAI
    public int ContextSize { get; set; } = 4096; // For LocalLlamaCpp
    public int Threads { get; set; } = Environment.ProcessorCount; // For LocalLlamaCpp
    public int? GpuLayers { get; set; } // For LocalLlamaCpp
}

/// <summary>
/// AI provider types.
/// </summary>
public enum AiProvider
{
    LocalLlamaCpp,
    OpenAIResponses,
    CustomHttp,
    Mock  // For testing/feasibility validation
}

