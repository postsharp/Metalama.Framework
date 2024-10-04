// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
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
        private readonly IProjectOptionsFactory _projectOptionsFactory;
        private readonly UserCodeInvoker _userCodeInvoker;
        private readonly DesignTimeExceptionHandler _exceptionHandler;

        static TheDiagnosticSuppressor()
        {
            DesignTimeServices.Initialize();
        }

        [UsedImplicitly]
        public TheDiagnosticSuppressor() : this(
            DesignTimeServiceProviderFactory.GetSharedServiceProvider<DesignTimeAnalysisProcessServiceProviderFactory>() ) { }

        public TheDiagnosticSuppressor( GlobalServiceProvider serviceProvider )
        {
            this._exceptionHandler = serviceProvider.GetRequiredService<DesignTimeExceptionHandler>();

            try
            {
                this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
                this._designTimeDiagnosticDefinitions = serviceProvider.GetRequiredService<IUserDiagnosticRegistrationService>().DiagnosticDefinitions;
                this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
                this._projectOptionsFactory = serviceProvider.GetRequiredService<IProjectOptionsFactory>();
                this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                this._exceptionHandler.ReportException( e );

                throw;
            }
        }

        public override void ReportSuppressions( SuppressionAnalysisContext context )
            => this.ReportSuppressions(
                new SuppressionAnalysisContextAdapter( context, this._projectOptionsFactory ),
                this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors );

        internal void ReportSuppressions(
            ISuppressionAnalysisContext context,
            ImmutableDictionary<string, SuppressionDescriptor> supportedSuppressionDescriptors )
        {
            if ( MetalamaCompilerInfo.IsActive || context.Compilation is not CSharpCompilation compilation )
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

                // Execute the pipeline.
                var pipelineResult = pipeline.Execute( compilation, cancellationToken );

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

                var compilationContext = compilation.GetCompilationContext();

                foreach ( var diagnosticGroup in diagnosticsBySyntaxTree )
                {
                    var syntaxTree = diagnosticGroup.Key;

                    var suppressions = pipelineResult.Value.GetSuppressionsOnSyntaxTree( syntaxTree.FilePath );

                    var designTimeSuppressions = suppressions
                        .Where( s => supportedSuppressionDescriptors.ContainsKey( s.Suppression.Definition.SuppressedDiagnosticId ) )
                        .ToReadOnlyList();

                    if ( designTimeSuppressions.Count == 0 )
                    {
                        continue;
                    }

                    var semanticModel = compilation.GetCachedSemanticModel( syntaxTree );

                    var suppressionsBySymbol =
                        ImmutableDictionaryOfArray<SerializableDeclarationId, ISuppression>.Create(
                            designTimeSuppressions,
                            s => s.DeclarationId,
                            s => s.Suppression );

                    foreach ( var diagnostic in diagnosticGroup )
                    {
                        var diagnosticNode = syntaxTree.GetRoot().FindNode( diagnostic.Location.SourceSpan, getInnermostNodeForTie: true );

                        var memberNode = diagnosticNode.FindSymbolDeclaringNode();

                        for ( var node = memberNode; node != null; node = node.Parent )
                        {
                            var symbol = semanticModel.GetDeclaredSymbol( node );

                            if ( symbol == null || !symbol.TryGetSerializableId( out var symbolId ) )
                            {
                                continue;
                            }

                            foreach ( var suppression in suppressionsBySymbol[symbolId]
                                         .Where(
                                             s => string.Equals( s.Definition.SuppressedDiagnosticId, diagnostic.Id, StringComparison.OrdinalIgnoreCase ) ) )
                            {
                                if ( suppression.Filter is { } filter )
                                {
                                    var executionContext = new UserCodeExecutionContext(
                                        pipeline.ServiceProvider,
                                        UserCodeDescription.Create( "evaluating suppression filter for {0} on {1}", suppression.Definition, symbolId ),
                                        compilationContext );

                                    var filterPassed = this._userCodeInvoker.Invoke(
                                        () => filter( SuppressionFactories.CreateDiagnostic( diagnostic ) ),
                                        executionContext );

                                    if ( !filterPassed )
                                    {
                                        continue;
                                    }
                                }

                                suppressionsCount++;

                                if ( supportedSuppressionDescriptors.TryGetValue(
                                        suppression.Definition.SuppressedDiagnosticId,
                                        out var suppressionDescriptor ) )
                                {
                                    context.ReportSuppression( Suppression.Create( suppressionDescriptor, diagnostic ) );
                                }
                                else
                                {
                                    // We can't report a warning here, but our design-time analyzer does it.
                                }
                            }
                        }
                    }
                }

                this._logger.Trace?.Log(
                    $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): {suppressionsCount} suppressions reported." );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                this._exceptionHandler.ReportException( e );
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}