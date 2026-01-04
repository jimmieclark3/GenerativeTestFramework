// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Text.Json;

namespace ReverseCoverage.PluginContracts.Validators;

/// <summary>
/// Validates JSON against schemas (simplified implementation).
/// </summary>
public static class JsonSchemaValidator
{
    /// <summary>
    /// Validates that the JSON can be deserialized to the expected type.
    /// </summary>
    public static bool Validate<T>(string json, out T? result, out string? errorMessage)
    {
        result = default;
        errorMessage = null;

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            result = JsonSerializer.Deserialize<T>(json, options);
            return result != null;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Validates JSON structure (basic validation).
    /// </summary>
    public static bool ValidateStructure(string json, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}

