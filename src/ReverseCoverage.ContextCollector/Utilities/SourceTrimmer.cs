// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace ReverseCoverage.ContextCollector.Utilities;

/// <summary>
/// Utilities for trimming source code to relevant regions.
/// </summary>
public static class SourceTrimmer
{
    private const int DefaultMaxLines = 200;
    private const int ContextLines = 5;

    /// <summary>
    /// Trims source code to include the specified line range with context.
    /// </summary>
    public static string TrimToRegion(string source, int startLine, int endLine, int maxLines = DefaultMaxLines)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        var lines = source.Split('\n');
        var totalLines = lines.Length;

        // Expand range with context
        var contextStart = Math.Max(0, startLine - ContextLines - 1);
        var contextEnd = Math.Min(totalLines - 1, endLine + ContextLines - 1);

        // If the method is small, include the full body
        var rangeSize = contextEnd - contextStart + 1;
        if (rangeSize <= maxLines)
        {
            return string.Join("\n", lines.Skip(contextStart).Take(rangeSize));
        }

        // Otherwise, trim to maxLines around the uncovered region
        var uncoveredSize = endLine - startLine + 1;
        if (uncoveredSize >= maxLines)
        {
            // Uncovered region is larger than max, just take the start
            return string.Join("\n", lines.Skip(contextStart).Take(maxLines));
        }

        // Center the uncovered region within maxLines
        var availableLines = maxLines - uncoveredSize;
        var beforeLines = availableLines / 2;
        var afterLines = availableLines - beforeLines;

        var trimStart = Math.Max(0, startLine - beforeLines - 1);
        var trimEnd = Math.Min(totalLines - 1, endLine + afterLines - 1);

        return string.Join("\n", lines.Skip(trimStart).Take(trimEnd - trimStart + 1));
    }

    /// <summary>
    /// Trims source code to a method body if it's small enough, otherwise includes context.
    /// </summary>
    public static string TrimMethod(string source, int maxLines = DefaultMaxLines)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        var lines = source.Split('\n');
        if (lines.Length <= maxLines)
        {
            return source;
        }

        // Include first maxLines
        return string.Join("\n", lines.Take(maxLines)) + "\n// ... (truncated)";
    }
}

