﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Metalama.Framework.Engine.Analyzers;

[DiagnosticAnalyzer( LanguageNames.CSharp )]
public partial class MetalamaInternalsAnalyzer : DiagnosticAnalyzer
{
    // Range: 820-839
    private static readonly DiagnosticDescriptor _cannotConsumeApi = new(
        "LAMA0820",
        "Cannot reference an internal Metalama API.",
        "Cannot reference the internal Metalama API '{0}'.",
        "Metalama",
        DiagnosticSeverity.Warning,
        true );

    private static readonly DiagnosticDescriptor _cannotExposeApi = new(
        "LAMA0821",
        "Cannot expose an internal Metalama API.",
        "Cannot expose the internal Metalama API '{0}'.",
        "Metalama",
        DiagnosticSeverity.Warning,
        true );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create( _cannotConsumeApi, _cannotExposeApi );

    public override void Initialize( AnalysisContext context )
    {
        //     Debugger.Launch();
        context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );
    }

    private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
    {
        var projectKind = ProjectClassifier.GetProjectKind( context.SemanticModel.Compilation.AssemblyName );

        switch ( projectKind )
        {
            case ProjectKind.MetalamaPublicApi:
                new PublicApiVisitor( context ).Visit( context.SemanticModel.SyntaxTree.GetRoot() );

                break;

            case ProjectKind.UserProject:
                new UserProjectVisitor( context ).Visit( context.SemanticModel.SyntaxTree.GetRoot() );

                break;
        }
    }
}