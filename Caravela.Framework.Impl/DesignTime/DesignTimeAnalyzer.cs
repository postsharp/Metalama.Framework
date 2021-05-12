﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticAnalyzer"/>. It reports all diagnostics that we produce.
    /// </summary>
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class DesignTimeAnalyzer : DiagnosticAnalyzer
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

#pragma warning disable RS1026 // Enable concurrent execution
        public override void Initialize( AnalysisContext context )
#pragma warning restore RS1026 // Enable concurrent execution
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
                var buildOptions = new BuildOptions( context.Options.AnalyzerConfigOptionsProvider );

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
                        result.SyntaxTree,
                        context.ReportDiagnostic,
                        true );
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
                var buildOptions = new BuildOptions( context.Options.AnalyzerConfigOptionsProvider );

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