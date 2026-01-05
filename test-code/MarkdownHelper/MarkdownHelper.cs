// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

namespace MarkdownHelper;

/// <summary>
/// Simple markdown parsing utilities.
/// </summary>
public static class MarkdownHelper
{
    /// <summary>
    /// Converts markdown bold (**text**) to HTML strong tags.
    /// </summary>
    public static string BoldToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown ?? string.Empty;
        }

        var result = markdown;
        var boldPattern = "**";
        var inBold = false;

        while (result.Contains(boldPattern))
        {
            var index = result.IndexOf(boldPattern);
            if (index == -1) break;

            if (!inBold)
            {
                result = result.Remove(index, 2);
                result = result.Insert(index, "<strong>");
                inBold = true;
            }
            else
            {
                result = result.Remove(index, 2);
                result = result.Insert(index, "</strong>");
                inBold = false;
            }
        }

        return result;
    }

    /// <summary>
    /// Converts markdown italic (*text*) to HTML em tags.
    /// </summary>
    public static string ItalicToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown ?? string.Empty;
        }

        var result = markdown;
        var italicPattern = "*";
        var inItalic = false;
        var i = 0;

        while (i < result.Length)
        {
            if (i < result.Length - 1 && result[i] == '*' && result[i + 1] == '*')
            {
                // Skip bold markers
                i += 2;
                continue;
            }

            if (result[i] == '*')
            {
                if (!inItalic)
                {
                    result = result.Remove(i, 1);
                    result = result.Insert(i, "<em>");
                    i += 4; // length of <em>
                    inItalic = true;
                }
                else
                {
                    result = result.Remove(i, 1);
                    result = result.Insert(i, "</em>");
                    i += 5; // length of </em>
                    inItalic = false;
                }
            }
            else
            {
                i++;
            }
        }

        return result;
    }

    /// <summary>
    /// Extracts all headers from markdown text.
    /// </summary>
    public static List<string> ExtractHeaders(string markdown)
    {
        var headers = new List<string>();

        if (string.IsNullOrWhiteSpace(markdown))
        {
            return headers;
        }

        var lines = markdown.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#"))
            {
                var headerText = trimmed.TrimStart('#').Trim();
                if (!string.IsNullOrWhiteSpace(headerText))
                {
                    headers.Add(headerText);
                }
            }
        }

        return headers;
    }

    /// <summary>
    /// Converts markdown links [text](url) to HTML anchor tags.
    /// </summary>
    public static string LinksToHtml(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return markdown ?? string.Empty;
        }

        var result = markdown;
        var pattern = "[";

        while (result.Contains(pattern))
        {
            var startBracket = result.IndexOf('[');
            if (startBracket == -1) break;

            var endBracket = result.IndexOf(']', startBracket);
            if (endBracket == -1) break;

            if (endBracket + 1 >= result.Length || result[endBracket + 1] != '(')
            {
                // Not a valid link, skip
                result = result.Remove(startBracket, 1);
                continue;
            }

            var startParen = endBracket + 1;
            var endParen = result.IndexOf(')', startParen);
            if (endParen == -1) break;

            var linkText = result.Substring(startBracket + 1, endBracket - startBracket - 1);
            var linkUrl = result.Substring(startParen + 1, endParen - startParen - 1);

            var htmlLink = $"<a href=\"{linkUrl}\">{linkText}</a>";
            result = result.Remove(startBracket, endParen - startBracket + 1);
            result = result.Insert(startBracket, htmlLink);
        }

        return result;
    }

    /// <summary>
    /// Counts the number of code blocks in markdown.
    /// </summary>
    public static int CountCodeBlocks(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return 0;
        }

        var count = 0;
        var lines = markdown.Split('\n');
        var inCodeBlock = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    count++;
                    inCodeBlock = true;
                }
                else
                {
                    inCodeBlock = false;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Strips all markdown formatting and returns plain text.
    /// </summary>
    public static string ToPlainText(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var result = markdown;

        // Remove headers
        result = System.Text.RegularExpressions.Regex.Replace(result, @"^#+\s*", "", 
            System.Text.RegularExpressions.RegexOptions.Multiline);

        // Remove bold
        result = result.Replace("**", "");

        // Remove italic
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(?<!\*)\*(?!\*)", "");

        // Remove links
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\[([^\]]+)\]\([^\)]+\)", "$1");

        // Remove code blocks
        result = result.Replace("```", "");

        // Remove inline code
        result = result.Replace("`", "");

        return result.Trim();
    }
}
