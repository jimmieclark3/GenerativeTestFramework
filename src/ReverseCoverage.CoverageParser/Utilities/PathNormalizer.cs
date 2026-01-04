// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.IO;

namespace ReverseCoverage.CoverageParser.Utilities;

/// <summary>
/// Normalizes file paths for cross-platform compatibility.
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Normalizes a file path to a consistent format (case-insensitive comparison, normalized separators).
    /// </summary>
    public static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // Convert to absolute path if relative
        var normalized = Path.GetFullPath(path);

        // Normalize separators to forward slashes for consistency
        normalized = normalized.Replace('\\', '/');

        // Remove trailing separators
        normalized = normalized.TrimEnd('/');

        return normalized;
    }

    /// <summary>
    /// Compares two paths for equality (case-insensitive, normalized).
    /// </summary>
    public static bool AreEqual(string path1, string path2)
    {
        return string.Equals(Normalize(path1), Normalize(path2), StringComparison.OrdinalIgnoreCase);
    }
}

