// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedType.Global

#pragma warning disable RS1026 // Enable concurrent execution
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#pragma warning disable RS1022 // Remove access to our implementation types 

namespace Metalama.Framework.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticAnalyzer"/>. It reports all diagnostics that we produce.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TheDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        private readonly ILogger _logger;
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        static TheDiagnosticAnalyzer()
        {
            DesignTimeServices.Initialize();
        }

        public TheDiagnosticAnalyzer() : this( DesignTimeServiceProviderFactory.GetServiceProvider() ) { }

        public TheDiagnosticAnalyzer( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
            this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => this._designTimeDiagnosticDefinitions.SupportedDiagnosticDescriptors.Values.ToImmutableArray();

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

            context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            try
            {
                var compilation = context.SemanticModel.Compilation;

                var syntaxTreeFilePath = context.SemanticModel.SyntaxTree.FilePath;

                this._logger.Trace?.Log(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}) started." );

                var projectOptions = MSBuildProjectOptions.GetInstance( context.Options.AnalyzerConfigOptionsProvider );

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    this._logger.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                // Execute the pipeline.
                var cancellationToken = context.CancellationToken.IgnoreIfDebugging();

                var pipeline = this._pipelineFactory.GetOrCreatePipeline(
                    projectOptions,
                    compilation,
                    cancellationToken );

                if ( pipeline == null )
                {
                    return;
                }

                var diagnosticCount = 0;

                // Execute the analysis that are not performed in the pipeline.
                void ReportDiagnostic( Diagnostic diagnostic )
                {
                    this._logger.Trace?.Log(
                        $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): TemplatingCodeValidator reported {diagnostic.Id}." );

                    diagnosticCount++;

                    context.ReportDiagnostic( diagnostic );
                }

                TemplatingCodeValidator.Validate(
                    pipeline.ServiceProvider,
                    context.SemanticModel,
                    ReportDiagnostic,
                    pipeline.MustReportPausedPipelineAsErrors && pipeline.IsCompileTimeSyntaxTreeOutdated( context.SemanticModel.SyntaxTree.FilePath ),
                    true,
                    cancellationToken );

                // Run the pipeline.
                if ( !this._pipelineFactory.TryExecute(
                        projectOptions,
                        compilation,
                        cancellationToken,
                        out var compilationResult ) )
                {
                    this._logger.Trace?.Log(
                        $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed." );

                    return;
                }

                var diagnostics = compilationResult.GetAllDiagnostics( syntaxTreeFilePath );
                var suppressions = compilationResult.GetAllSuppressions( syntaxTreeFilePath );

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
                            diagnosticCount++;

                            context.ReportDiagnostic(
                                DesignTimeDiagnosticDescriptors.UnregisteredSuppression.CreateRoslynDiagnostic(
                                    location,
                                    (Id: suppression.Definition.SuppressedDiagnosticId, symbol) ) );
                        }
                    }
                }

                this._logger.Trace?.Log(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): completed. {diagnosticCount} diagnostic(s) reported." );
            }
            catch ( Exception e )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }
    }
}