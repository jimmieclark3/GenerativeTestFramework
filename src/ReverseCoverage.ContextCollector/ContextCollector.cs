// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using System.Globalization;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using ReverseCoverage.ContextCollector.Analyzers;
using ReverseCoverage.ContextCollector.Utilities;
using ReverseCoverage.TargetModel.Models;
using ReverseCoverage.TargetModel.Utilities;

namespace ReverseCoverage.ContextCollector;

/// <summary>
/// Uses Roslyn to gather method context, signatures, and branch hints for uncovered code.
/// </summary>
public class ContextCollector : IDisposable
{
    private MSBuildWorkspace? _workspace;
    private Solution? _solution;
    private bool _disposed;

    public ContextCollector()
    {
        // Register MSBuild if not already registered
        if (!MSBuildLocator.IsRegistered)
        {
            var instances = MSBuildLocator.QueryVisualStudioInstances().OrderByDescending(x => x.Version);
            if (instances.Any())
            {
                var instance = instances.First();
                MSBuildLocator.RegisterInstance(instance);
                Console.WriteLine($"Registered MSBuild instance: {instance.Version} at {instance.MSBuildPath}");
            }
            else
            {
                Console.WriteLine("Warning: No MSBuild instances found. Trying to use default .NET SDK...");
            }
        }
    }

    /// <summary>
    /// Loads a solution or project for analysis.
    /// </summary>
    public async Task LoadSolutionAsync(string solutionOrProjectPath, CancellationToken cancellationToken = default)
    {
        // Use absolute path
        var absolutePath = Path.IsPathRooted(solutionOrProjectPath) 
            ? solutionOrProjectPath 
            : Path.Combine(Directory.GetCurrentDirectory(), solutionOrProjectPath);
        
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Solution/project file not found: {absolutePath}");
        }
        
        _workspace = MSBuildWorkspace.Create();
        
        // Set workspace properties to help with loading
        _workspace.WorkspaceFailed += (sender, args) =>
        {
            // Only show errors, not warnings (warnings are common and usually harmless)
            if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
            {
                Console.WriteLine($"Workspace error: {args.Diagnostic.Message}");
            }
        };
        
        if (absolutePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Loading solution: {absolutePath}");
            _solution = await _workspace.OpenSolutionAsync(absolutePath, cancellationToken: cancellationToken);
            Console.WriteLine($"Solution loaded with {_solution.Projects.Count()} projects");
            
            // List project names for debugging
            foreach (var proj in _solution.Projects)
            {
                Console.WriteLine($"  - {proj.Name} ({proj.Language})");
            }
        }
        else
        {
            Console.WriteLine($"Loading project: {absolutePath}");
            var project = await _workspace.OpenProjectAsync(absolutePath, cancellationToken: cancellationToken);
            _solution = project.Solution;
            Console.WriteLine($"Project loaded, solution has {_solution.Projects.Count()} projects");
        }
    }

    /// <summary>
    /// Collects context for a method target and creates a generation request.
    /// </summary>
    public async Task<GenerationRequest> CollectContextAsync(
        MethodTarget target,
        GenerationConstraints constraints,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString("N");

        // Try to find the method in the solution first
        IMethodSymbol? methodSymbol = null;
        if (_solution != null && _solution.Projects.Any())
        {
            methodSymbol = await FindMethodSymbolAsync(target, cancellationToken);
        }
        
        // If solution loading failed, parse from source file directly
        if (methodSymbol == null && target.SourceFiles != null && target.SourceFiles.Length > 0)
        {
            methodSymbol = await FindMethodSymbolFromFileAsync(target, cancellationToken);
        }
        
        if (methodSymbol == null)
        {
            throw new InvalidOperationException($"Could not find method {target.MethodDisplayName} in solution or source files.");
        }

        // Extract method signature
        var methodSignature = methodSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Get containing type source
        var containingTypeSource = await GetContainingTypeSourceAsync(methodSymbol, cancellationToken);

        // Get method source
        var methodSource = await GetMethodSourceAsync(methodSymbol, cancellationToken);

        // Extract branch hints
        var branchHints = await ExtractBranchHints(methodSymbol, cancellationToken);

        // Build harness plan
        var harnessPlan = await BuildHarnessPlan(methodSymbol, cancellationToken);

        return new GenerationRequest
        {
            RequestId = requestId,
            TargetMethod = target,
            MethodSignature = methodSignature,
            ContainingTypeSource = containingTypeSource,
            MethodSource = methodSource,
            BranchHints = branchHints,
            HarnessPlan = harnessPlan,
            Constraints = constraints
        };
    }

    private async Task<IMethodSymbol?> FindMethodSymbolAsync(MethodTarget target, CancellationToken cancellationToken)
    {
        if (_solution == null) return null;

        // Extract method name from MethodDisplayName (e.g., "DemoCalc.DemoCalc.Evaluate" -> "Evaluate")
        var methodNameFromDisplay = target.MethodDisplayName.Contains('.') 
            ? target.MethodDisplayName.Split('.').Last()
            : target.MethodDisplayName;
        
        var targetTypeSimpleName = target.TypeFullName.Split('.').Last();

        // Try to find by source file first (most reliable)
        if (target.SourceFiles != null && target.SourceFiles.Length > 0)
        {
            var sourceFileName = Path.GetFileName(target.SourceFiles[0]);
            
            // Search all projects for the file - prioritize projects with matching names
            var projectsToSearch = _solution.Projects
                .OrderByDescending(p => p.Name.Contains(targetTypeSimpleName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            foreach (var project in projectsToSearch)
            {
                try
                {
                    var compilation = await project.GetCompilationAsync(cancellationToken);
                    if (compilation == null) continue;
                    
                    // Find syntax tree by filename - try exact match first
                    foreach (var syntaxTree in compilation.SyntaxTrees)
                    {
                        var treeFileName = Path.GetFileName(syntaxTree.FilePath);
                        
                        // Match by filename (case-insensitive)
                        if (treeFileName.Equals(sourceFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Found the file! Now find the type and method
                            var semanticModel = compilation.GetSemanticModel(syntaxTree);
                            var root = await syntaxTree.GetRootAsync(cancellationToken);
                            var typeDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>();
                            
                            foreach (var typeDecl in typeDeclarations)
                            {
                                if (typeDecl.Identifier.ValueText == targetTypeSimpleName)
                                {
                                    var type = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                                    if (type != null)
                                    {
                                        // Found the type! Now find the method
                                        foreach (var member in type.GetMembers())
                                        {
                                            if (member is IMethodSymbol method && method.Name == methodNameFromDisplay)
                                            {
                                                return method;
                                            }
                                        }
                                        // If we found the type but not the method by exact name, return first method with matching name
                                        var methods = type.GetMembers().OfType<IMethodSymbol>()
                                            .Where(m => m.Name == methodNameFromDisplay)
                                            .FirstOrDefault();
                                        if (methods != null)
                                        {
                                            return methods;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log but continue searching other projects
                    System.Diagnostics.Debug.WriteLine($"Error processing project {project.Name}: {ex.Message}");
                }
            }
        }

        // Fallback: search all projects and types
        foreach (var project in _solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null) continue;
            
            // Get all types in the compilation
            var allTypesInProject = compilation.GlobalNamespace.GetNamespaceMembers()
                .SelectMany(ns => GetAllTypes(ns))
                .ToList();
            
            // Try to find type by searching all types - check both namespace and simple name
            INamedTypeSymbol? type = null;
            
            // First, try to find by source file if available
            if (target.SourceFiles != null && target.SourceFiles.Length > 0)
            {
                foreach (var sourceFile in target.SourceFiles)
                {
                    // Match by filename (most reliable since paths can be malformed)
                    var sourceFileName = Path.GetFileName(sourceFile);
                    
                    foreach (var syntaxTree in compilation.SyntaxTrees)
                    {
                        var treeFileName = Path.GetFileName(syntaxTree.FilePath);
                        
                        // Match by filename
                        if (treeFileName.Equals(sourceFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            var semanticModel = compilation.GetSemanticModel(syntaxTree);
                            var root = await syntaxTree.GetRootAsync(cancellationToken);
                            var typeDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>();
                            
                            foreach (var typeDecl in typeDeclarations)
                            {
                                if (typeDecl.Identifier.ValueText == targetTypeSimpleName)
                                {
                                    type = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                                    if (type != null) break;
                                }
                            }
                            if (type != null) break;
                        }
                    }
                    if (type != null) break;
                }
            }
            
            // Fallback: search all types
            if (type == null)
            {
                foreach (var t in allTypesInProject)
                {
                    var typeDisplayName = t.ToDisplayString();
                    var typeMetadataName = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var typeQualifiedName = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    
                    // Try multiple matching strategies
                    if (typeDisplayName == target.TypeFullName || 
                        typeDisplayName.EndsWith("." + targetTypeSimpleName) ||
                        typeMetadataName.Contains(target.TypeFullName) ||
                        typeQualifiedName.Contains(target.TypeFullName) ||
                        (t.Name == targetTypeSimpleName && (typeDisplayName.Contains("DemoCalc") || typeDisplayName == targetTypeSimpleName)))
                    {
                        type = t;
                        break;
                    }
                }
            }

            // Fallback: try GetTypeByMetadataName with variations
            if (type == null)
            {
                var typeNameVariations = new[]
                {
                    target.TypeFullName, // "DemoCalc.DemoCalc"
                    target.TypeFullName.Split('.').Last(), // "DemoCalc"
                    "DemoCalc.DemoCalc", // Explicit
                    "DemoCalc" // Simple name
                };

                foreach (var typeName in typeNameVariations)
                {
                    type = compilation.GetTypeByMetadataName(typeName);
                    if (type != null) break;
                }
            }
            
            // Last resort: search all types by simple name match
            if (type == null)
            {
                type = allTypesInProject.FirstOrDefault(t => t.Name == targetTypeSimpleName);
            }

            if (type == null) continue;

            // Try to find method by matching methodId
            IMethodSymbol? fallbackMethod = null;
            foreach (var member in type.GetMembers())
            {
                if (member is IMethodSymbol method)
                {
                    var methodId = MethodIdHasher.CreateMethodId(
                        project.AssemblyName ?? string.Empty,
                        target.TypeFullName,
                        method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

                    if (methodId == target.MethodId)
                    {
                        return method;
                    }
                    
                    // Fallback: match by method name
                    if (method.Name == methodNameFromDisplay && fallbackMethod == null)
                    {
                        fallbackMethod = method;
                    }
                }
            }
            
            // If we found a method by name but not by ID, use it as fallback
            if (fallbackMethod != null)
            {
                return fallbackMethod;
            }
        }

        return null;
    }
    
    private async Task<IMethodSymbol?> FindMethodSymbolFromFileAsync(
        MethodTarget target,
        CancellationToken cancellationToken)
    {
        if (target.SourceFiles == null || target.SourceFiles.Length == 0)
        {
            return null;
        }
        
        var sourceFile = target.SourceFiles[0];
        if (!File.Exists(sourceFile))
        {
            return null;
        }
        
        try
        {
            var content = await File.ReadAllTextAsync(sourceFile, cancellationToken);
            var tree = CSharpSyntaxTree.ParseText(content, path: sourceFile);
            var root = await tree.GetRootAsync(cancellationToken);
            
            // Create a simple compilation to get semantic model
            var compilation = CSharpCompilation.Create("Temp")
                .AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            
            var semanticModel = compilation.GetSemanticModel(tree);
            
            // Find the method by name
            var methodName = target.MethodDisplayName.Contains('.') 
                ? target.MethodDisplayName.Split('.').Last()
                : target.MethodDisplayName;
            
            var methodDecl = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == methodName);
            
            if (methodDecl != null)
            {
                return semanticModel.GetDeclaredSymbol(methodDecl);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing method from file {sourceFile}: {ex.Message}");
        }
        
        return null;
    }

    /// <summary>
    /// Finds all public methods in the solution that should be tested.
    /// Falls back to direct file parsing if MSBuild workspace fails.
    /// </summary>
    public async Task<List<MethodTarget>> FindAllMethodsAsync(
        string? projectNameFilter = null,
        CancellationToken cancellationToken = default)
    {
        var methods = new List<MethodTarget>();
        
        // Try MSBuild workspace first
        if (_solution != null && _solution.Projects.Any())
        {
            return await FindAllMethodsFromWorkspaceAsync(projectNameFilter, cancellationToken);
        }
        
        // Fallback: Parse source files directly
        Console.WriteLine("MSBuild workspace not available, parsing source files directly...");
        return await FindAllMethodsFromFilesAsync(projectNameFilter, cancellationToken);
    }
    
    private async Task<List<MethodTarget>> FindAllMethodsFromWorkspaceAsync(
        string? projectNameFilter,
        CancellationToken cancellationToken)
    {
        var methods = new List<MethodTarget>();
        
        Console.WriteLine($"Solution has {_solution!.Projects.Count()} projects");
        
        foreach (var project in _solution.Projects)
        {
            Console.WriteLine($"  Checking project: {project.Name}");
            
            // Filter by project name if specified
            if (!string.IsNullOrEmpty(projectNameFilter) && 
                !project.Name.Contains(projectNameFilter, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"    Skipped (doesn't match filter: {projectNameFilter})");
                continue;
            }

            // Skip test projects
            if (project.Name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase) ||
                (project.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) && 
                 !project.Name.Equals(projectNameFilter, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"    Skipped (test project)");
                continue;
            }

            Console.WriteLine($"    Processing project: {project.Name}");
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null)
            {
                Console.WriteLine($"    Compilation is null, skipping");
                continue;
            }
            
            Console.WriteLine($"    Compilation loaded, assembly: {compilation.AssemblyName}");

            // Get all types in the project - need to traverse the entire namespace tree
            var allTypes = new List<INamedTypeSymbol>();
            
            // Start from global namespace and recursively get all types
            void CollectTypes(INamespaceSymbol ns)
            {
                foreach (var type in ns.GetTypeMembers())
                {
                    allTypes.Add(type);
                }
                
                foreach (var nestedNs in ns.GetNamespaceMembers())
                {
                    CollectTypes(nestedNs);
                }
            }
            
            CollectTypes(compilation.GlobalNamespace);
            
            Console.WriteLine($"    Found {allTypes.Count} types in project {project.Name}");

            foreach (var type in allTypes)
            {
                // Skip test projects and generated code
                if (type.Name.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) ||
                    type.Name.Contains("Generated", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                // Skip types from designer files
                var typeLocation = type.Locations.FirstOrDefault();
                if (typeLocation != null && typeLocation.SourceTree != null)
                {
                    var fileName = Path.GetFileName(typeLocation.SourceTree.FilePath);
                    if (fileName.Contains("Designer", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                // Get all methods (public, internal, private, protected) - including handlers
                foreach (var member in type.GetMembers())
                {
                    if (member is IMethodSymbol method && 
                        (method.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Public ||
                         method.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Internal ||
                         method.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Private ||
                         method.DeclaredAccessibility == Microsoft.CodeAnalysis.Accessibility.Protected) &&
                        !method.IsAbstract &&
                        !method.IsImplicitlyDeclared &&
                        (method.MethodKind == Microsoft.CodeAnalysis.MethodKind.Ordinary ||
                         method.MethodKind == Microsoft.CodeAnalysis.MethodKind.EventRaise) &&
                        !method.Name.StartsWith("get_") &&
                        !method.Name.StartsWith("set_"))
                    {
                        var methodId = ReverseCoverage.TargetModel.Utilities.MethodIdHasher.CreateMethodId(
                            compilation.AssemblyName ?? project.Name,
                            type.ToDisplayString(),
                            method.ToDisplayString());

                        // Get source file
                        var sourceFiles = new List<string>();
                        if (method.DeclaringSyntaxReferences.Length > 0)
                        {
                            var syntaxRef = method.DeclaringSyntaxReferences[0];
                            if (syntaxRef.SyntaxTree != null)
                            {
                                sourceFiles.Add(syntaxRef.SyntaxTree.FilePath);
                            }
                        }

                        methods.Add(new MethodTarget
                        {
                            MethodId = methodId,
                            TypeFullName = type.ToDisplayString(),
                            MethodDisplayName = $"{type.Name}.{method.Name}",
                            SourceFiles = sourceFiles.ToArray(),
                            UncoveredSequencePoints = Array.Empty<ReverseCoverage.TargetModel.Models.SequencePoint>(),
                            UncoveredBranchPoints = Array.Empty<ReverseCoverage.TargetModel.Models.BranchPoint>()
                        });
                    }
                }
            }
        }

        return methods;
    }
    
    private async Task<List<MethodTarget>> FindAllMethodsFromFilesAsync(
        string? projectNameFilter,
        CancellationToken cancellationToken)
    {
        var methods = new List<MethodTarget>();
        
        // Find source project directory from test project path
        // For now, search in test-code folder for .cs files
        var testCodeDir = Path.Combine(Directory.GetCurrentDirectory(), "test-code");
        if (!Directory.Exists(testCodeDir))
        {
            testCodeDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "test-code");
        }
        
        if (!Directory.Exists(testCodeDir))
        {
            Console.WriteLine($"Could not find test-code directory. Current dir: {Directory.GetCurrentDirectory()}");
            return methods;
        }
        
        Console.WriteLine($"Searching for source files in: {testCodeDir}");
        
        // Find all .cs files in test-code, excluding Tests projects, designer files, and generated code
        var sourceFiles = Directory.GetFiles(testCodeDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(".Tests") && 
                       !f.Contains("\\bin\\") && 
                       !f.Contains("\\obj\\") &&
                       !f.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) &&
                       !f.Contains("\\Generated\\", StringComparison.OrdinalIgnoreCase) &&
                       !Path.GetFileName(f).Contains("Designer", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        Console.WriteLine($"Found {sourceFiles.Count} source files");
        
        // Parse each file to find methods
        foreach (var file in sourceFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var tree = CSharpSyntaxTree.ParseText(content, path: file);
                var root = await tree.GetRootAsync(cancellationToken);
                
                // Find all method declarations (including private, protected, internal - all methods including handlers)
                var methodDeclarations = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => !m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AbstractKeyword)))
                    .ToList();
                
                foreach (var methodDecl in methodDeclarations)
                {
                    var className = methodDecl.Parent is TypeDeclarationSyntax typeDecl ? typeDecl.Identifier.ValueText : "Unknown";
                    var methodName = methodDecl.Identifier.ValueText;
                    
                    // Skip property accessors (get_/set_) but include event handlers (they're real methods)
                    if (methodName.StartsWith("get_") || methodName.StartsWith("set_"))
                    {
                        continue;
                    }
                    
                    var methodId = ReverseCoverage.TargetModel.Utilities.MethodIdHasher.CreateMethodId(
                        Path.GetFileNameWithoutExtension(file),
                        className,
                        methodName);
                    
                    methods.Add(new MethodTarget
                    {
                        MethodId = methodId,
                        TypeFullName = className,
                        MethodDisplayName = $"{className}.{methodName}",
                        SourceFiles = new[] { file },
                        UncoveredSequencePoints = Array.Empty<ReverseCoverage.TargetModel.Models.SequencePoint>(),
                        UncoveredBranchPoints = Array.Empty<ReverseCoverage.TargetModel.Models.BranchPoint>()
                    });
                    
                    Console.WriteLine($"  Found method: {className}.{methodName} in {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error parsing {file}: {ex.Message}");
            }
        }
        
        return methods;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetAllTypes(nestedNamespace))
            {
                yield return type;
            }
        }
    }

    private async Task<string> GetContainingTypeSourceAsync(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        var syntaxReference = methodSymbol.ContainingType.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null) return string.Empty;

        var syntaxTree = await syntaxReference.SyntaxTree.GetRootAsync(cancellationToken);
        var typeNode = syntaxTree.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .FirstOrDefault(n => n.Identifier.ValueText == methodSymbol.ContainingType.Name);

        return typeNode?.ToFullString() ?? string.Empty;
    }

    private async Task<string> GetMethodSourceAsync(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null) return string.Empty;

        var syntaxTree = await syntaxReference.SyntaxTree.GetRootAsync(cancellationToken);
        var methodNode = syntaxTree.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(n => n.Identifier.ValueText == methodSymbol.Name);

        if (methodNode == null) return string.Empty;

        var source = methodNode.ToFullString();
        return SourceTrimmer.TrimMethod(source);
    }

    private async Task<BranchHint[]> ExtractBranchHints(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        if (_solution == null) return Array.Empty<BranchHint>();

        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null) return Array.Empty<BranchHint>();

        var syntaxTree = syntaxReference.SyntaxTree;
        var root = await syntaxTree.GetRootAsync(cancellationToken);
        var methodNode = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(n => n.Identifier.ValueText == methodSymbol.Name);

        if (methodNode == null) return Array.Empty<BranchHint>();

        // Get semantic model from the compilation
        var project = _solution.Projects.FirstOrDefault(p => 
            p.GetCompilationAsync(cancellationToken).Result?.GetTypeByMetadataName(methodSymbol.ContainingType.ToDisplayString()) != null);
        
        if (project == null) return Array.Empty<BranchHint>();

        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null) return Array.Empty<BranchHint>();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        return BranchHintExtractor.ExtractBranchHints(methodNode, semanticModel);
    }

    private async Task<HarnessPlan> BuildHarnessPlan(IMethodSymbol methodSymbol, CancellationToken cancellationToken)
    {
        if (_solution == null)
        {
            // Fallback: create a basic harness plan
            return new HarnessPlan
            {
                ConstructStrategy = methodSymbol.IsStatic ? "Static" : "PublicCtor",
                Dependencies = Array.Empty<DependencyPlan>()
            };
        }

        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
        {
            return new HarnessPlan
            {
                ConstructStrategy = methodSymbol.IsStatic ? "Static" : "PublicCtor",
                Dependencies = Array.Empty<DependencyPlan>()
            };
        }

        var syntaxTree = syntaxReference.SyntaxTree;
        var project = _solution.Projects.FirstOrDefault(p => 
            p.GetCompilationAsync(cancellationToken).Result?.GetTypeByMetadataName(methodSymbol.ContainingType.ToDisplayString()) != null);
        
        if (project == null)
        {
            return new HarnessPlan
            {
                ConstructStrategy = methodSymbol.IsStatic ? "Static" : "PublicCtor",
                Dependencies = Array.Empty<DependencyPlan>()
            };
        }

        var compilation = await project.GetCompilationAsync(cancellationToken);
        if (compilation == null)
        {
            return new HarnessPlan
            {
                ConstructStrategy = methodSymbol.IsStatic ? "Static" : "PublicCtor",
                Dependencies = Array.Empty<DependencyPlan>()
            };
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        return HarnessPlanBuilder.BuildHarnessPlan(methodSymbol, semanticModel);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _workspace?.Dispose();
            _disposed = true;
        }
    }
}

