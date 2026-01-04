// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Globalization;
using System.Xml.Linq;
using ReverseCoverage.CoverageParser.Utilities;
using ReverseCoverage.TargetModel.Models;
using ReverseCoverage.TargetModel.Utilities;

namespace ReverseCoverage.CoverageParser.Parsers;

/// <summary>
/// Parser for Cobertura XML coverage format.
/// </summary>
public static class CoberturaParser
{
    public static TargetMap Parse(string[] coverageXmlPaths, string? sourceCommit = null)
    {
        var modules = new List<ModuleTarget>();
        var generatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        foreach (var xmlPath in coverageXmlPaths)
        {
            if (!File.Exists(xmlPath))
                continue;

            var doc = XDocument.Load(xmlPath);
            var coverage = doc.Element("coverage");
            if (coverage == null)
                continue;

            var packages = coverage.Elements("packages")?.Elements("package") ?? Enumerable.Empty<XElement>();

            foreach (var package in packages)
            {
                var assemblyName = package.Attribute("name")?.Value ?? string.Empty;
                var classes = package.Elements("classes")?.Elements("class") ?? Enumerable.Empty<XElement>();

                var methods = new List<MethodTarget>();

                foreach (var classElement in classes)
                {
                    var className = classElement.Attribute("name")?.Value ?? string.Empty;
                    var fileName = classElement.Attribute("filename")?.Value ?? string.Empty;
                    var normalizedFileName = PathNormalizer.Normalize(fileName);

                    var classMethods = classElement.Elements("methods")?.Elements("method") ?? Enumerable.Empty<XElement>();

                    foreach (var methodElement in classMethods)
                    {
                        var methodName = methodElement.Attribute("name")?.Value ?? string.Empty;
                        var methodSignature = methodElement.Attribute("signature")?.Value ?? string.Empty;

                        var methodId = MethodIdHasher.CreateMethodId(assemblyName, className, $"{methodName}{methodSignature}");

                        var lines = methodElement.Elements("lines")?.Elements("line") ?? Enumerable.Empty<XElement>();
                        var uncoveredSequencePoints = new List<SequencePoint>();
                        var uncoveredBranchPoints = new List<BranchPoint>();

                        foreach (var line in lines)
                        {
                            var lineNumber = int.Parse(line.Attribute("number")?.Value ?? "0");
                            var hits = int.Parse(line.Attribute("hits")?.Value ?? "0");
                            var branch = line.Attribute("branch")?.Value == "True";
                            var conditionCoverage = line.Attribute("condition-coverage")?.Value ?? string.Empty;

                            // Uncovered sequence point
                            if (hits == 0)
                            {
                                uncoveredSequencePoints.Add(new SequencePoint
                                {
                                    File = normalizedFileName,
                                    StartLine = lineNumber,
                                    EndLine = lineNumber
                                });
                            }

                            // Branch points
                            if (branch)
                            {
                                var branches = line.Elements("conditions")?.Elements("condition") ?? Enumerable.Empty<XElement>();
                                var branchIndex = 0;
                                foreach (var condition in branches)
                                {
                                    var conditionHits = int.Parse(condition.Attribute("coverage")?.Value ?? "0");
                                    if (conditionHits == 0)
                                    {
                                        uncoveredBranchPoints.Add(new BranchPoint
                                        {
                                            File = normalizedFileName,
                                            Line = lineNumber,
                                            PathOrdinal = branchIndex
                                        });
                                    }
                                    branchIndex++;
                                }
                            }
                        }

                        if (uncoveredSequencePoints.Count > 0 || uncoveredBranchPoints.Count > 0)
                        {
                            methods.Add(new MethodTarget
                            {
                                MethodId = methodId,
                                TypeFullName = className,
                                MethodDisplayName = $"{className}.{methodName}",
                                SourceFiles = new[] { normalizedFileName },
                                UncoveredSequencePoints = uncoveredSequencePoints.ToArray(),
                                UncoveredBranchPoints = uncoveredBranchPoints.ToArray()
                            });
                        }
                    }
                }

                if (methods.Count > 0)
                {
                    modules.Add(new ModuleTarget
                    {
                        AssemblyName = assemblyName,
                        AssemblyPath = assemblyName, // Cobertura doesn't provide full path
                        Methods = methods.ToArray()
                    });
                }
            }
        }

        return new TargetMap
        {
            SourceCommit = sourceCommit,
            GeneratedAtUtc = generatedAtUtc,
            Modules = modules.ToArray()
        };
    }
}

