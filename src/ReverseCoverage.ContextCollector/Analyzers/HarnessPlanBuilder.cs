// Copyright (c) 2026 Test Synthesis Framework
// Licensed under the MIT License

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReverseCoverage.TargetModel.Models;

namespace ReverseCoverage.ContextCollector.Analyzers;

/// <summary>
/// Builds harness plans for constructing test instances.
/// </summary>
public static class HarnessPlanBuilder
{
    public static HarnessPlan BuildHarnessPlan(IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        var containingType = methodSymbol.ContainingType;
        var dependencies = new List<DependencyPlan>();

        // Determine construction strategy
        string constructStrategy;
        string? ctorOrFactorySignature = null;

        if (methodSymbol.IsStatic)
        {
            constructStrategy = "Static";
        }
        else
        {
            // Look for public constructor
            var publicCtor = containingType.Constructors
                .FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length > 0);

            if (publicCtor != null)
            {
                constructStrategy = "PublicCtor";
                ctorOrFactorySignature = publicCtor.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
                // Analyze constructor dependencies
                foreach (var param in publicCtor.Parameters)
                {
                    dependencies.Add(CreateDependencyPlan(param));
                }
            }
            else
            {
                var internalCtor = containingType.Constructors
                    .FirstOrDefault(c => c.DeclaredAccessibility == Accessibility.Internal && c.Parameters.Length > 0);

                if (internalCtor != null)
                {
                    constructStrategy = "InternalCtor";
                    ctorOrFactorySignature = internalCtor.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    
                    foreach (var param in internalCtor.Parameters)
                    {
                        dependencies.Add(CreateDependencyPlan(param));
                    }
                }
                else if (containingType.Constructors.Any(c => c.Parameters.Length == 0))
                {
                    constructStrategy = "PublicCtor";
                }
                else
                {
                    constructStrategy = "NotCallable";
                }
            }
        }

        return new HarnessPlan
        {
            ConstructStrategy = constructStrategy,
            CtorOrFactorySignature = ctorOrFactorySignature,
            Dependencies = dependencies.ToArray()
        };
    }

    private static DependencyPlan CreateDependencyPlan(IParameterSymbol parameter)
    {
        var typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var strategy = DetermineDependencyStrategy(parameter.Type);

        return new DependencyPlan
        {
            ParameterName = parameter.Name,
            TypeName = typeName,
            Strategy = strategy,
            Notes = strategy == "NeedsAdapter" ? "May require adapter for IO/time/random dependencies" : null
        };
    }

    private static string DetermineDependencyStrategy(ITypeSymbol type)
    {
        // Check if it's an interface
        if (type.TypeKind == TypeKind.Interface)
        {
            return "AutoMock";
        }

        // Check if it's a primitive or simple type
        if (type.SpecialType != SpecialType.None)
        {
            return "UseDefaultValue";
        }

        // Check for IO/time/random dependencies
        var typeName = type.ToDisplayString();
        if (typeName.Contains("Stream") || 
            typeName.Contains("File") || 
            typeName.Contains("Directory") ||
            typeName.Contains("DateTime") ||
            typeName.Contains("Random") ||
            typeName.Contains("HttpClient") ||
            typeName.Contains("Network"))
        {
            return "NeedsAdapter";
        }

        // Default to UseFake for complex types
        return "UseFake";
    }
}

