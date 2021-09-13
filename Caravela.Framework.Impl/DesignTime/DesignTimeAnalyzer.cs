// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable UnusedType.Global

#pragma warning disable RS1026 // Enable concurrent execution
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#pragma warning disable RS1022 // Remove access to our implementation types 

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticAnalyzer"/>. It reports all diagnostics that we produce.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DesignTimeAnalyzer : DiagnosticAnalyzer
    {
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => this._designTimeDiagnosticDefinitions.SupportedDiagnosticDescriptors.Values.ToImmutableArray();

        public override void Initialize( AnalysisContext context )
        {
            if ( CaravelaCompilerInfo.IsActive )
            {
                // This analyzer should work only at design time.
                return;
            }
            else
            {
                DebuggingHelper.RequireCaravelaCompiler();
            }

            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );

            // Semantic model analysis is used for frequent and "short loop" analysis, principally of the templates themselves.
            context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );

            context.RegisterCompilationAction( this.AnalyzeCompilation );
        }

        private void AnalyzeCompilation( CompilationAnalysisContext context )
        {
            Logger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeCompilation('{context.Compilation.AssemblyName}') started." );

            try { }
            catch ( Exception e )
            {
                Logger.Instance?.Write( e.ToString() );
            }
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            try
            {
                // Execute the analysis that are not performed in the pipeline.
                var buildOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                Logger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeSemanticModel('{context.SemanticModel.SyntaxTree.FilePath}')" );

                DebuggingHelper.AttachDebugger( buildOptions );

                // Execute the pipeline.
                var compilation = context.SemanticModel.Compilation;

                var syntaxTreeResults = DesignTimeAspectPipelineCache.Instance.GetSyntaxTreeResults(
                    compilation,
                    new[] { context.SemanticModel.SyntaxTree },
                    buildOptions,
                    context.CancellationToken );

                // Report diagnostics from the pipeline.
                foreach ( var result in syntaxTreeResults )
                {
                    Logger.Instance?.Write(
                        $"DesignTimeAnalyzer.AnalyzeSemanticModel('{context.SemanticModel.SyntaxTree.FilePath}'): {result.Diagnostics.Length} diagnostics reported on '{result.SyntaxTree.FilePath}'." );

                    DesignTimeDiagnosticHelper.ReportDiagnostics(
                        result.Diagnostics,
                        compilation,
                        context.ReportDiagnostic,
                        true );

                    // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                    foreach ( var suppression in result.Suppressions.Where(
                        s => !this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) ) )
                    {
                        foreach ( var symbol in DocumentationCommentId.GetSymbolsForDeclarationId( suppression.SymbolId, compilation ) )
                        {
                            var location = symbol.GetDiagnosticLocation();

                            if ( location is not null )
                            {
                                context.ReportDiagnostic(
                                    DesignTimeDiagnosticDescriptors.UnregisteredSuppression.CreateDiagnostic(
                                        location,
                                        (Id: suppression.Definition.SuppressedDiagnosticId, symbol) ) );
                            }
                        }
                    }
                }

                // Perform additional analysis not done by the design-time pipeline.
                var pipeline = DesignTimeAspectPipelineCache.Instance.GetOrCreatePipeline( buildOptions, context.CancellationToken );

                if ( pipeline != null )
                {
                    TemplatingCodeValidator.Validate(
                        context.SemanticModel,
                        context.ReportDiagnostic,
                        pipeline,
                        context.CancellationToken );
                }
            }
            catch ( Exception e )
            {
                Logger.Instance?.Write( e.ToString() );
            }
        }
    }
}