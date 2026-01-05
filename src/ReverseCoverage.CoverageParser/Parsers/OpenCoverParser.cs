// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Globalization;
using System.Xml.Linq;
using ReverseCoverage.CoverageParser.Utilities;
using ReverseCoverage.TargetModel.Models;
using ReverseCoverage.TargetModel.Utilities;

namespace ReverseCoverage.CoverageParser.Parsers;

/// <summary>
/// Parser for OpenCover XML coverage format.
/// </summary>
public static class OpenCoverParser
{
    public static TargetMap Parse(string[] coverageXmlPaths, string? sourceCommit = null)
    {
        var modules = new List<ModuleTarget>();
        var generatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        var fileMap = new Dictionary<string, string>(); // fileid -> path

        foreach (var xmlPath in coverageXmlPaths)
        {
            if (!File.Exists(xmlPath))
                continue;

            var doc = XDocument.Load(xmlPath);
            var coverageSession = doc.Element("CoverageSession");
            if (coverageSession == null)
                continue;

            // Build file map
            var files = coverageSession.Element("Files")?.Elements("File") ?? Enumerable.Empty<XElement>();
            foreach (var file in files)
            {
                var fileId = file.Attribute("uid")?.Value ?? string.Empty;
                var filePath = file.Attribute("fullPath")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(fileId) && !string.IsNullOrEmpty(filePath))
                {
                    fileMap[fileId] = PathNormalizer.Normalize(filePath);
                }
            }

            var modulesElement = coverageSession.Element("Modules")?.Elements("Module") ?? Enumerable.Empty<XElement>();

            foreach (var moduleElement in modulesElement)
            {
                var assemblyName = moduleElement.Element("ModuleName")?.Value ?? string.Empty;
                var modulePath = moduleElement.Element("ModulePath")?.Value ?? string.Empty;

                var classes = moduleElement.Element("Classes")?.Elements("Class") ?? Enumerable.Empty<XElement>();

                var methods = new List<MethodTarget>();

                foreach (var classElement in classes)
                {
                    var className = classElement.Element("FullName")?.Value ?? string.Empty;
                    var classMethods = classElement.Element("Methods")?.Elements("Method") ?? Enumerable.Empty<XElement>();

                    foreach (var methodElement in classMethods)
                    {
                        var methodName = methodElement.Element("Name")?.Value ?? string.Empty;
                        var methodSignature = methodElement.Element("Signature")?.Value ?? string.Empty;

                        var methodId = MethodIdHasher.CreateMethodId(assemblyName, className, $"{methodName}{methodSignature}");

                        var sequencePoints = methodElement.Element("SequencePoints")?.Elements("SequencePoint") ?? Enumerable.Empty<XElement>();
                        var branchPoints = methodElement.Element("BranchPoints")?.Elements("BranchPoint") ?? Enumerable.Empty<XElement>();

                        var uncoveredSequencePoints = new List<SequencePoint>();
                        var uncoveredBranchPoints = new List<BranchPoint>();
                        var sourceFiles = new HashSet<string>();

                        foreach (var seqPoint in sequencePoints)
                        {
                            var fileId = seqPoint.Attribute("fileid")?.Value ?? string.Empty;
                            var startLine = int.Parse(seqPoint.Attribute("sl")?.Value ?? "0");
                            var startCol = int.Parse(seqPoint.Attribute("sc")?.Value ?? "0");
                            var endLine = int.Parse(seqPoint.Attribute("el")?.Value ?? "0");
                            var endCol = int.Parse(seqPoint.Attribute("ec")?.Value ?? "0");
                            var visits = int.Parse(seqPoint.Attribute("vc")?.Value ?? "0");

                            if (visits == 0 && fileMap.TryGetValue(fileId, out var filePath))
                            {
                                uncoveredSequencePoints.Add(new SequencePoint
                                {
                                    File = filePath,
                                    StartLine = startLine,
                                    EndLine = endLine,
                                    StartCol = startCol > 0 ? startCol : null,
                                    EndCol = endCol > 0 ? endCol : null
                                });
                                sourceFiles.Add(filePath);
                            }
                        }

                        foreach (var branchPoint in branchPoints)
                        {
                            var fileId = branchPoint.Attribute("fileid")?.Value ?? string.Empty;
                            var line = int.Parse(branchPoint.Attribute("sl")?.Value ?? "0");
                            var path = int.Parse(branchPoint.Attribute("path")?.Value ?? "0");
                            var visits = int.Parse(branchPoint.Attribute("vc")?.Value ?? "0");
                            var offset = int.Parse(branchPoint.Attribute("offset")?.Value ?? "0");

                            if (visits == 0 && fileMap.TryGetValue(fileId, out var filePath))
                            {
                                uncoveredBranchPoints.Add(new BranchPoint
                                {
                                    File = filePath,
                                    Line = line,
                                    PathOrdinal = path,
                                    Offset = offset > 0 ? offset : null
                                });
                                sourceFiles.Add(filePath);
                            }
                        }

                        if (uncoveredSequencePoints.Count > 0 || uncoveredBranchPoints.Count > 0)
                        {
                            methods.Add(new MethodTarget
                            {
                                MethodId = methodId,
                                TypeFullName = className,
                                MethodDisplayName = $"{className}.{methodName}",
                                SourceFiles = sourceFiles.ToArray(),
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
                        AssemblyPath = modulePath,
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

