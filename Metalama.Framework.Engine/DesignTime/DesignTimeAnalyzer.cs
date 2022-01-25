// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Engine.DesignTime.Diagnostics;
using Metalama.Framework.Engine.DesignTime.Pipeline;
using Metalama.Framework.Engine.DesignTime.Utilities;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
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

namespace Metalama.Framework.Engine.DesignTime
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

        static DesignTimeAnalyzer()
        {
            DiagnosticsService.Initialize( DebuggingHelper.ProcessKind );
        }

        public override void Initialize( AnalysisContext context )
        {
            if ( MetalamaCompilerInfo.IsActive )
            {
                // This analyzer should work only at design time.
                return;
            }
            else
            {
                DebuggingHelper.RequireMetalamaCompiler();
            }

            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );

            context.RegisterCompilationAction( this.AnalyzeCompilation );
        }

        private void AnalyzeCompilation( CompilationAnalysisContext context )
        {
            Logger.DesignTime.Trace?.Log(
                $"DesignTimeAnalyzer.AnalyzeCompilation('{context.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( context.Compilation )}) started." );

            try
            {
                // Execute the analysis that are not performed in the pipeline.
                var projectOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    Logger.DesignTime.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                // Execute the pipeline.
                var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

                var pipeline = DesignTimeAspectPipelineFactory.Instance.GetOrCreatePipeline(
                    projectOptions,
                    context.Compilation,
                    cancellationToken );

                if ( pipeline == null )
                {
                    return;
                }

                var compilation = context.Compilation;

                if ( !DesignTimeAspectPipelineFactory.Instance.TryExecute(
                        projectOptions,
                        compilation,
                        cancellationToken,
                        out var compilationResult ) )
                {
                    Logger.DesignTime.Trace?.Log(
                        $"DesignTimeAnalyzer.AnalyzeCompilation('{context.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( context.Compilation )}): the pipeline failed." );

                    return;
                }

                var diagnostics = compilationResult.GetAllDiagnostics();
                var suppressions = compilationResult.GetAllSuppressions();

                // Report diagnostics.
                DesignTimeDiagnosticHelper.ReportDiagnostics(
                    diagnostics,
                    compilation,
                    context.ReportDiagnostic,
                    true );

                // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                foreach ( var suppression in suppressions.Where(
                             s => !this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) ) )
                {
                    foreach ( var symbol in DocumentationCommentId.GetSymbolsForDeclarationId( suppression.SymbolId, compilation ) )
                    {
                        var location = symbol.GetDiagnosticLocation();

                        if ( location is not null )
                        {
                            context.ReportDiagnostic(
                                DesignTimeDiagnosticDescriptors.UnregisteredSuppression.CreateRoslynDiagnostic(
                                    location,
                                    (Id: suppression.Definition.SuppressedDiagnosticId, symbol) ) );
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }
    }
}