﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public partial class DesignTimeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnosticDescriptors;

        public static ImmutableHashSet<string> DesignTimeDiagnosticIds { get; }

        static DesignTimeAnalyzer()
        {
            CompilerServiceProvider.Initialize();

            _supportedDiagnosticDescriptors = DiagnosticDescriptorHelper
                .GetDiagnosticDescriptors(
                    typeof(TemplatingDiagnosticDescriptors),
                    typeof(DesignTimeDiagnosticDescriptors),
                    typeof(GeneralDiagnosticDescriptors),
                    typeof(SerializationDiagnosticDescriptors) )
                .ToImmutableArray();

            DesignTimeDiagnosticIds = _supportedDiagnosticDescriptors.Select( x => x.Id ).ToImmutableHashSet();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnosticDescriptors;

        public override void Initialize( AnalysisContext context )
        {
            if ( CaravelaCompilerInfo.IsActive )
            {
                // This analyzer should work only at design time.
                return;
            }

            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.ReportDiagnostics );

            // Semantic model analysis is used for frequent and "short loop" analysis, principally of the templates themselves.
            context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            Visitor visitor = new( context );
            visitor.Visit( context.SemanticModel.SyntaxTree.GetRoot() );

            // -

            var compilation = (CSharpCompilation) context.SemanticModel.Compilation;

            // Execute the pipeline.
            var syntaxTreeResults = DesignTimeAspectPipelineCache.GetPipelineResult(
                context.SemanticModel.Compilation,
                new[] { context.SemanticModel.SyntaxTree },
                new BuildOptions( context.Options.AnalyzerConfigOptionsProvider ),
                context.CancellationToken,
                true );

            // Report diagnostics.
            var result = syntaxTreeResults.SingleOrDefault( r => r != null && r.SyntaxTree.FilePath == context.SemanticModel.SyntaxTree.FilePath );

            if ( result != null )
            {
                DesignTimeDiagnosticHelper.ReportDiagnostics(
                    result.Diagnostics,
                    compilation,
                    context.ReportDiagnostic,
                    true,
                    context.SemanticModel.SyntaxTree );
            }
        }
    }
}