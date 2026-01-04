// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using ReverseCoverage.CoverageParser.Parsers;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.CoverageParser;

/// <summary>
/// Parses coverage XML files (Cobertura/OpenCover) into normalized TargetMap.
/// </summary>
public class CoverageParser
{
    /// <summary>
    /// Parses coverage XML files and returns a normalized TargetMap.
    /// </summary>
    public async Task<TargetMap> ParseCoverageAsync(
        string[] coverageXmlPaths,
        string? sourceCommit = null,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            // Determine format by examining file extension or content
            var coberturaFiles = new List<string>();
            var openCoverFiles = new List<string>();

            foreach (var path in coverageXmlPaths)
            {
                var fileName = Path.GetFileName(path).ToLowerInvariant();
                if (fileName.Contains("cobertura") || fileName.EndsWith(".cobertura.xml"))
                {
                    coberturaFiles.Add(path);
                }
                else if (fileName.Contains("opencover") || fileName.EndsWith(".opencover.xml"))
                {
                    openCoverFiles.Add(path);
                }
                else
                {
                    // Try to detect by examining first few lines
                    try
                    {
                        var firstLine = File.ReadLines(path).FirstOrDefault() ?? string.Empty;
                        if (firstLine.Contains("coverage") && firstLine.Contains("line-rate"))
                        {
                            coberturaFiles.Add(path);
                        }
                        else if (firstLine.Contains("CoverageSession"))
                        {
                            openCoverFiles.Add(path);
                        }
                        else
                        {
                            // Default to Cobertura
                            coberturaFiles.Add(path);
                        }
                    }
                    catch
                    {
                        // Default to Cobertura on error
                        coberturaFiles.Add(path);
                    }
                }
            }

            var allModules = new List<ModuleTarget>();

            // Parse Cobertura files
            if (coberturaFiles.Count > 0)
            {
                var coberturaMap = CoberturaParser.Parse(coberturaFiles.ToArray(), sourceCommit);
                allModules.AddRange(coberturaMap.Modules);
            }

            // Parse OpenCover files
            if (openCoverFiles.Count > 0)
            {
                var openCoverMap = OpenCoverParser.Parse(openCoverFiles.ToArray(), sourceCommit);
                allModules.AddRange(openCoverMap.Modules);
            }

            // Merge modules with the same assembly name
            var mergedModules = allModules
                .GroupBy(m => m.AssemblyName)
                .Select(g => new ModuleTarget
                {
                    AssemblyName = g.Key,
                    AssemblyPath = g.First().AssemblyPath,
                    Methods = g.SelectMany(m => m.Methods).ToArray()
                })
                .ToArray();

            return new TargetMap
            {
                SourceCommit = sourceCommit,
                GeneratedAtUtc = DateTime.UtcNow.ToString("O"),
                Modules = mergedModules
            };
        }, cancellationToken);
    }
}

