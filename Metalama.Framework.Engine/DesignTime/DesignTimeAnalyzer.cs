// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.DesignTime.Diagnostics;
using Metalama.Framework.Engine.DesignTime.Pipeline;
using Metalama.Framework.Engine.DesignTime.Utilities;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating;
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
            Logger.Initialize();
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
                var projectOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                var syntaxTreeFilePath = context.SemanticModel.SyntaxTree.FilePath;
                Logger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}')" );

                DebuggingHelper.AttachDebugger( projectOptions );

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    Logger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                // Execute the pipeline.
                var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

                var pipeline = DesignTimeAspectPipelineFactory.Instance.GetOrCreatePipeline(
                    projectOptions,
                    context.SemanticModel.Compilation,
                    cancellationToken );

                if ( pipeline == null )
                {
                    return;
                }

                var compilation = context.SemanticModel.Compilation;

                if ( !DesignTimeAspectPipelineFactory.Instance.TryExecute(
                        projectOptions,
                        compilation,
                        cancellationToken,
                        out var compilationResult ) )
                {
                    Logger.Instance?.Write( $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}'): the pipeline failed." );

                    return;
                }

                var pipelineDiagnostics = compilationResult.GetDiagnosticsOnSyntaxTree( syntaxTreeFilePath );

                // Execute validators.
                var validatorDiagnostics = ImmutableUserDiagnosticList.Empty;

                if ( compilationResult.HasValidators )
                {
                    var project = pipeline.LatestConfiguration?.ProjectModel;

                    if ( project != null )
                    {
                        DesignTimeValidatorRunner validatorRunner = new( compilationResult, project );
                        validatorDiagnostics = validatorRunner.Validate( context.SemanticModel, cancellationToken );
                    }
                }

                // TODO: suppressions from validators.

                // Report diagnostics from the pipeline.
                Logger.Instance?.Write(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}'): {pipelineDiagnostics.Diagnostics.Length} diagnostics and {pipelineDiagnostics.Suppressions.Length} suppressions reported on '{syntaxTreeFilePath}'." );

                DesignTimeDiagnosticHelper.ReportDiagnostics(
                    pipelineDiagnostics.Diagnostics.Concat( validatorDiagnostics.ReportedDiagnostics ),
                    compilation,
                    context.ReportDiagnostic,
                    true );

                // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                foreach ( var suppression in pipelineDiagnostics.Suppressions.Where(
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

                // Perform additional analysis not done by the design-time pipeline.
                TemplatingCodeValidator.Validate(
                    context.SemanticModel,
                    context.ReportDiagnostic,
                    pipeline,
                    cancellationToken );
            }
            catch ( Exception e )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }
    }
}