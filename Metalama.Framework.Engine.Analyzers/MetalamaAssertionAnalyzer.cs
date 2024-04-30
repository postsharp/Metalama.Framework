// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Operations;
using System;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Metalama.Framework.Engine.Analyzers;

[DiagnosticAnalyzer( LanguageNames.CSharp )]
[UsedImplicitly]
public class MetalamaAssertionAnalyzer : DiagnosticAnalyzer
{
    // Range: 0840-0849
    internal static readonly DiagnosticDescriptor AssertNotNullShouldNotBeUsedOnSymbols = new(
        "LAMA0840",
        "AssertNotNull should not be used to assert ISymbol.",
        "The AssertNotNull method does not provide the user the information about introduced type not being yet supported in this particular scenario.",
        "Metalama",
        DiagnosticSeverity.Warning,
        true );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create( AssertNotNullShouldNotBeUsedOnSymbols );

    public override void Initialize( AnalysisContext context )
    {
        context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.ReportDiagnostics );
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction( InitializeCompilation );
    }

    private static void InitializeCompilation( CompilationStartAnalysisContext context )
    {
        string[] ignoredNamespaces = [
            "Metalama.Framework.Engine.Aspects",
            "Metalama.Framework.Engine.CompileTime",
            "Metalama.Framework.Engine.DesignTime",
            "Metalama.Framework.Engine.Linking",
            ];

        var invariantSymbol =
            context.Compilation.GetTypeByMetadataName( "Metalama.Framework.Engine.Invariant" )!;

        var iSymbolSymbol =
            context.Compilation.GetTypeByMetadataName( "Microsoft.CodeAnalysis.ISymbol" )!;

        context.RegisterOperationAction( context => AnalyzeInvariant(context, invariantSymbol, iSymbolSymbol, ignoredNamespaces ), OperationKind.Invocation, OperationKind.MethodReference );
    }

    private static void AnalyzeInvariant( OperationAnalysisContext context, INamedTypeSymbol invariantTypeSymbol, INamedTypeSymbol iSymbolSymbol, string[]  namespaces)
    {
        var method = context.Operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IMethodReferenceOperation methodReference => methodReference.Method,
            _ => throw new InvalidOperationException( $"Unexpected operation type '{context.Operation.GetType()}'." )
        };

        var containingNamespace = context.ContainingSymbol.ContainingNamespace.ToString();

        if ( SymbolEqualityComparer.Default.Equals( invariantTypeSymbol, method.ContainingType) 
             && method.Name == "AssertNotNull" && method.TypeArguments.Length > 0
             && namespaces.All( n => !containingNamespace.StartsWith(n, StringComparison.Ordinal)))
        {
            var typeArgument = method.TypeArguments[0];

            if ( context.Compilation.ClassifyConversion( typeArgument, iSymbolSymbol ) is { IsIdentity: true } or { IsImplicit: true, IsReference: true } )
            {
                context.ReportDiagnostic( Diagnostic.Create( AssertNotNullShouldNotBeUsedOnSymbols, context.Operation.Syntax.GetLocation() ) );
            }
        }
    }
}