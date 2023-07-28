// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
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
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions;

        private readonly ILogger _logger;
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        static TheDiagnosticSuppressor()
        {
            DesignTimeServices.Initialize();
        }

        [UsedImplicitly]
        public TheDiagnosticSuppressor() : this(
            DesignTimeServiceProviderFactory.GetSharedServiceProvider<DesignTimeAnalysisProcessServiceProviderFactory>() ) { }

        public TheDiagnosticSuppressor( GlobalServiceProvider serviceProvider )
        {
            try
            {
                this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
                this._designTimeDiagnosticDefinitions = serviceProvider.GetRequiredService<IUserDiagnosticRegistrationService>().DiagnosticDefinitions;
                this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );

                throw;
            }
        }

        public override void ReportSuppressions( SuppressionAnalysisContext context )
            => this.ReportSuppressions(
                new SuppressionAnalysisContextAdapter( context ),
                this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors );

        internal void ReportSuppressions(
            ISuppressionAnalysisContext context,
            ImmutableDictionary<string, SuppressionDescriptor> supportedSuppressionDescriptors )
        {
            if ( MetalamaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                this._logger.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{context.Compilation.AssemblyName}')." );

                var buildOptions = context.ProjectOptions;

                if ( !buildOptions.IsDesignTimeEnabled )
                {
                    this._logger.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                var cancellationToken = context.CancellationToken.IgnoreIfDebugging().ToTestable();

                var pipeline = this._pipelineFactory.GetOrCreatePipeline( context.ProjectOptions, compilation, cancellationToken );

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
                    context.ReportedDiagnostics.Where( d => d.Location.SourceTree != null )
                        .GroupBy( d => d.Location.SourceTree! );

                foreach ( var diagnosticGroup in diagnosticsBySyntaxTree )
                {
                    var syntaxTree = diagnosticGroup.Key;

                    var suppressions = pipelineResult.Value.GetDiagnosticsOnSyntaxTree( syntaxTree.FilePath ).Suppressions;

                    var designTimeSuppressions = suppressions.Where( s => supportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) )
                        .ToList();

                    if ( designTimeSuppressions.Count == 0 )
                    {
                        continue;
                    }

                    var semanticModel = compilation.GetCachedSemanticModel( syntaxTree );

                    var suppressionsBySymbol =
                        ImmutableDictionaryOfArray<SerializableDeclarationId, CacheableScopedSuppression>.Create(
                            designTimeSuppressions,
                            s => s.DeclarationId );

                    foreach ( var diagnostic in diagnosticGroup )
                    {
                        var diagnosticNode = syntaxTree.GetRoot().FindNode( diagnostic.Location.SourceSpan );

                        // Get the node that declares the ISymbol.
                        var memberNode = diagnosticNode.FindSymbolDeclaringNode();

                        if ( memberNode == null )
                        {
                            continue;
                        }

                        var symbol = semanticModel.GetDeclaredSymbol( memberNode );

                        if ( symbol == null )
                        {
                            continue;
                        }

                        if ( !symbol.TryGetSerializableId( out var symbolId ) )
                        {
                            continue;
                        }

                        foreach ( var suppression in suppressionsBySymbol[symbolId]
                                     .Where( s => string.Equals( s.Definition.SuppressedDiagnosticId, diagnostic.Id, StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            suppressionsCount++;

                            if ( supportedSuppressionDescriptors.TryGetValue(
                                    suppression.Definition.SuppressedDiagnosticId,
                                    out var suppressionDescriptor ) )
                            {
                                context.ReportSuppression( Suppression.Create( suppressionDescriptor, diagnostic ) );
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
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}