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
                MSBuildLocator.RegisterInstance(instances.First());
            }
        }
    }

    /// <summary>
    /// Loads a solution or project for analysis.
    /// </summary>
    public async Task LoadSolutionAsync(string solutionOrProjectPath, CancellationToken cancellationToken = default)
    {
        _workspace = MSBuildWorkspace.Create();
        
        if (solutionOrProjectPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            _solution = await _workspace.OpenSolutionAsync(solutionOrProjectPath, cancellationToken: cancellationToken);
        }
        else
        {
            var project = await _workspace.OpenProjectAsync(solutionOrProjectPath, cancellationToken: cancellationToken);
            _solution = project.Solution;
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
        if (_solution == null)
        {
            throw new InvalidOperationException("Solution must be loaded before collecting context.");
        }

        var requestId = Guid.NewGuid().ToString("N");

        // Find the method in the solution
        var methodSymbol = await FindMethodSymbolAsync(target, cancellationToken);
        if (methodSymbol == null)
        {
            throw new InvalidOperationException($"Could not find method {target.MethodDisplayName} in solution.");
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

        foreach (var project in _solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation == null) continue;

            var type = compilation.GetTypeByMetadataName(target.TypeFullName);
            if (type == null) continue;

            // Try to find method by matching methodId
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
                }
            }
        }

        return null;
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

