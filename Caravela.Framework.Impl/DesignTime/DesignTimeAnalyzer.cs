﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

#pragma warning disable RS1026 // Enable concurrent execution

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticAnalyzer"/>. It reports all diagnostics that we produce.
    /// </summary>
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

            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.ReportDiagnostics );

            // Semantic model analysis is used for frequent and "short loop" analysis, principally of the templates themselves.
            context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );

            context.RegisterCompilationAction( this.AnalyzeCompilation );
        }

        private void AnalyzeCompilation( CompilationAnalysisContext context )
        {
            DesignTimeLogger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeCompilation('{context.Compilation.AssemblyName}') started." );

            try
            {
                var buildOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                // Execute the pipeline.
                var syntaxTreeResults = DesignTimeAspectPipelineCache.Instance.GetDesignTimeResults(
                    context.Compilation,
                    context.Compilation.SyntaxTrees.ToList(),
                    buildOptions,
                    context.CancellationToken );

                // Report diagnostics from the pipeline.
                foreach ( var result in syntaxTreeResults.SyntaxTreeResults )
                {
                    DesignTimeLogger.Instance?.Write(
                        $"DesignTimeAnalyzer.AnalyzeCompilation('{context.Compilation.AssemblyName}'): {result.Diagnostics.Length} diagnostics reported on '{result.SyntaxTree.FilePath}'." );

                    DesignTimeDiagnosticHelper.ReportDiagnostics(
                        result.Diagnostics,
                        context.Compilation,
                        context.ReportDiagnostic,
                        true );

                    // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                    foreach ( var suppression in result.Suppressions.Where(
                        s => !this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) ) )
                    {
                        foreach ( var symbol in DocumentationCommentId.GetSymbolsForDeclarationId( suppression.SymbolId, context.Compilation ) )
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
            }
            catch ( Exception e )
            {
                DesignTimeLogger.Instance?.Write( e.ToString() );
            }
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            try
            {
                // Execute the analysis that are not performed in the pipeline.
                var buildOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                DesignTimeLogger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeSemanticModel('{context.SemanticModel.SyntaxTree.FilePath}')" );

                DesignTimeDebugger.AttachDebugger( buildOptions );

                // Additional validations that run out of the pipeline.
                DesignTimeAnalyzerAdditionalVisitor visitor = new( context, buildOptions );
                visitor.Visit( context.SemanticModel.SyntaxTree.GetRoot() );
            }
            catch ( Exception e )
            {
                DesignTimeLogger.Instance?.Write( e.ToString() );
            }
        }
    }
}