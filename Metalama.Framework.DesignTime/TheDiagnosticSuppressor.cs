// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#pragma warning disable RS1022 // Remove access to our implementation types.

namespace Metalama.Framework.DesignTime
{
    // ReSharper disable UnusedType.Global

    /// <summary>
    /// Our implementation of <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TheDiagnosticSuppressor : DiagnosticSuppressor
    {
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        private readonly ILogger _logger;
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        static TheDiagnosticSuppressor()
        {
            DesignTimeServices.Initialize();
        }

        public TheDiagnosticSuppressor() : this( DesignTimeServiceProviderFactory.GetServiceProvider( false ) ) { }

        public TheDiagnosticSuppressor( IServiceProvider serviceProvider )
        {
            try
            {
                this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
                this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );

                throw;
            }
        }

        public override void ReportSuppressions( SuppressionAnalysisContext context )
        {
            if ( MetalamaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                this._logger.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{context.Compilation.AssemblyName}')." );

                var buildOptions = MSBuildProjectOptionsFactory.Default.GetInstance( context.Options.AnalyzerConfigOptionsProvider );

                if ( !buildOptions.IsDesignTimeEnabled )
                {
                    this._logger.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                this.ReportSuppressions(
                    compilation,
                    context.ReportedDiagnostics,
                    context.ReportSuppression,
                    buildOptions,
                    context.CancellationToken.IgnoreIfDebugging() );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        /// <summary>
        /// A testable overload of <see cref="ReportSuppressions(SuppressionAnalysisContext)"/>.
        /// </summary>
        private void ReportSuppressions(
            Compilation compilation,
            ImmutableArray<Diagnostic> reportedDiagnostics,
            Action<Suppression> reportSuppression,
            MSBuildProjectOptions options,
            CancellationToken cancellationToken )
        {
            var pipeline = this._pipelineFactory.GetOrCreatePipeline( options, compilation, cancellationToken );

            if ( pipeline == null )
            {
                this._logger.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): cannot get the pipeline." );

                return;
            }

            var pipelineResult = pipeline.Execute( compilation, cancellationToken );

            // Execute the pipeline.
            if ( !pipelineResult.IsSuccessful )
            {
                this._logger.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): the pipeline failed." );

                return;
            }

            var suppressionsCount = 0;

            cancellationToken.ThrowIfCancellationRequested();

            var diagnosticsBySyntaxTree =
                reportedDiagnostics.Where( d => d.Location.SourceTree != null )
                    .GroupBy( d => d.Location.SourceTree! );

            foreach ( var diagnosticGroup in diagnosticsBySyntaxTree )
            {
                var syntaxTree = diagnosticGroup.Key;

                var suppressions = pipelineResult.Value.GetDiagnosticsOnSyntaxTree( syntaxTree.FilePath ).Suppressions;

                var designTimeSuppressions = suppressions.Where(
                        s => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) )
                    .ToList();

                if ( designTimeSuppressions.Count == 0 )
                {
                    continue;
                }

                var semanticModel = compilation.GetCachedSemanticModel( syntaxTree );

                var suppressionsBySymbol =
                    ImmutableDictionaryOfArray<string, CacheableScopedSuppression>.Create(
                        designTimeSuppressions,
                        s => s.SymbolId );

                foreach ( var diagnostic in diagnosticGroup )
                {
                    var diagnosticNode = syntaxTree.GetRoot().FindNode( diagnostic.Location.SourceSpan );
                    var symbol = semanticModel.GetDeclaredSymbol( diagnosticNode );

                    if ( symbol == null )
                    {
                        continue;
                    }

                    var symbolId = symbol.GetDocumentationCommentId().AssertNotNull();

                    foreach ( var suppression in suppressionsBySymbol[symbolId]
                                 .Where( s => string.Equals( s.Definition.SuppressedDiagnosticId, diagnostic.Id, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        suppressionsCount++;

                        if ( this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.TryGetValue(
                                suppression.Definition.SuppressedDiagnosticId,
                                out var suppressionDescriptor ) )
                        {
                            reportSuppression( Suppression.Create( suppressionDescriptor, diagnostic ) );
                        }
                        else
                        {
                            // We can't report a warning here, but DesignTimeAnalyzer does it.
                        }
                    }
                }
            }

            this._logger.Trace?.Log(
                $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): {suppressionsCount} suppressions reported." );
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}