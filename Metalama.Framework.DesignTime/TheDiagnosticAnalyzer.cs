﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CancellationTokenExtensions = Metalama.Framework.Engine.Utilities.Threading.CancellationTokenExtensions;

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
    public class TheDiagnosticAnalyzer : DefinitionOnlyDiagnosticAnalyzer
    {
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

        protected ILogger Logger { get; }

        public TheDiagnosticAnalyzer() : this( DesignTimeServiceProviderFactory.GetServiceProvider( false ) ) { }

        public TheDiagnosticAnalyzer( GlobalServiceProvider serviceProvider )
        {
            this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
            this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
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

            context.RegisterSemanticModelAction( this.AnalyzeSemanticModel );
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            try
            {
                var compilation = context.SemanticModel.Compilation;

                var syntaxTreeFilePath = context.SemanticModel.SyntaxTree.FilePath;

                this.Logger.Trace?.Log(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}) started." );

                var projectOptions = MSBuildProjectOptionsFactory.Default.GetInstance( context.Options.AnalyzerConfigOptionsProvider );

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    this.Logger.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                // Execute the pipeline.
                var cancellationToken = CancellationTokenExtensions.ToTestable( context.CancellationToken.IgnoreIfDebugging() );

                var pipeline = this._pipelineFactory.GetOrCreatePipeline(
                    projectOptions,
                    compilation,
                    CancellationTokenExtensions.ToTestable( cancellationToken ) );

                if ( pipeline == null )
                {
                    return;
                }

                var diagnosticCount = 0;

                // Execute the analysis that are not performed in the pipeline.
                void ReportDiagnostic( Diagnostic diagnostic )
                {
                    this.Logger.Trace?.Log( $"Reporting {FormatDiagnostic( diagnostic )}." );

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
                IEnumerable<Diagnostic> diagnostics;
                IEnumerable<CacheableScopedSuppression> suppressions;

                var pipelineResult = pipeline.Execute( compilation, cancellationToken );

                var filteredPipelineDiagnostics = pipelineResult.Diagnostics.Where( d => d.Location.SourceTree?.FilePath == syntaxTreeFilePath );

                if ( !pipelineResult.IsSuccessful )
                {
                    this.Logger.Trace?.Log(
                        $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed. It returned {pipelineResult.Diagnostics.Length} diagnostics." );

                    diagnostics = filteredPipelineDiagnostics;
                    suppressions = Enumerable.Empty<CacheableScopedSuppression>();
                }
                else
                {
                    diagnostics = pipelineResult.Value.GetAllDiagnostics( syntaxTreeFilePath ).Concat( filteredPipelineDiagnostics );
                    suppressions = pipelineResult.Value.GetAllSuppressions( syntaxTreeFilePath );
                }

                // Report diagnostics.
                this.ReportDiagnostics(
                    diagnostics,
                    compilation,
                    ReportDiagnostic,
                    true );

                // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                foreach ( var suppression in suppressions.Where(
                             s => !this.DiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) ) )
                {
                    foreach ( var symbol in DocumentationCommentId.GetSymbolsForDeclarationId( suppression.SymbolId, compilation ) )
                    {
                        var location = symbol.GetDiagnosticLocation();

                        if ( location is not null )
                        {
                            ReportDiagnostic(
                                DesignTimeDiagnosticDescriptors.UnregisteredSuppression.CreateRoslynDiagnostic(
                                    location,
                                    (Id: suppression.Definition.SuppressedDiagnosticId, symbol) ) );
                        }
                    }
                }

                this.Logger.Trace?.Log(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): completed. {diagnosticCount} diagnostic(s) reported." );
            }
            catch ( Exception e )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        /// <summary>
        /// Reports a list diagnostics in a <see cref="SyntaxTree"/>. The diagnostics may have been produced for an older version of the <see cref="SyntaxTree"/>.
        /// In this case, the method computes the new location of the syntax tree. Diagnostics of unknown types are wrapped into well-known diagnostics.
        /// </summary>
        /// <param name="diagnostics">List of diagnostics to be reported.</param>
        /// <param name="compilation">The compilation in which diagnostics must be reported.</param>
        /// <param name="reportDiagnostic">The delegate to call to report a diagnostic.</param>
        /// <param name="wrapUnknownDiagnostics">Determines whether unknown diagnostics should be wrapped into known diagnostics.</param>
        private void ReportDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            Compilation compilation,
            Action<Diagnostic> reportDiagnostic,
            bool wrapUnknownDiagnostics )
        {
            foreach ( var diagnostic in diagnostics )
            {
                Diagnostic designTimeDiagnostic;

                if ( !wrapUnknownDiagnostics || DesignTimeDiagnosticDefinitions.GetInstance().SupportedDiagnosticDescriptors.ContainsKey( diagnostic.Id ) )
                {
                    designTimeDiagnostic = diagnostic;
                }
                else
                {
                    this.Logger.Warning?.Log( $"The diagnostic {diagnostic.Id} is not supported at design time. Wrapping it." );

                    var descriptor =
                        diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => DesignTimeDiagnosticDescriptors.UserError,
                            DiagnosticSeverity.Hidden => DesignTimeDiagnosticDescriptors.UserHidden,
                            DiagnosticSeverity.Warning => DesignTimeDiagnosticDescriptors.UserWarning,
                            DiagnosticSeverity.Info => DesignTimeDiagnosticDescriptors.UserInfo,
                            _ => throw new NotImplementedException()
                        };

                    designTimeDiagnostic = descriptor.CreateRoslynDiagnostic(
                        diagnostic.Location,
                        (diagnostic.Id, diagnostic.GetMessage( CultureInfo.CurrentCulture )),
                        properties: diagnostic.Properties );
                }

                var reportSourceTree = designTimeDiagnostic.Location.SourceTree;

                if ( reportSourceTree == null || compilation.ContainsSyntaxTree( reportSourceTree ) )
                {
                    this.Logger.Trace?.Log( $"Reporting {FormatDiagnostic( designTimeDiagnostic )}" );
                    reportDiagnostic( designTimeDiagnostic );
                }
                else
                {
                    this.Logger.Trace?.Log( "Finding the source tree." );

                    // Find the new syntax tree in the compilation.

                    // TODO: Optimize the indexation of the syntax trees of a compilation. We're doing that many times in many methods and we
                    // could have a weak dictionary mapping a compilation to an index of syntax trees.
                    var newSyntaxTree = compilation.SyntaxTrees.Single( t => t.FilePath == reportSourceTree.FilePath );

                    // Find the node in the new syntax tree corresponding to the node in the old syntax tree.
                    var oldNode = reportSourceTree.GetRoot().FindNode( diagnostic.Location.SourceSpan, getInnermostNodeForTie: true );

                    if ( !NodeFinder.TryFindOldNodeInNewTree( oldNode, newSyntaxTree, out var newNode ) )
                    {
                        // We could not find the old node in the new tree. This should not happen if cache invalidation is correct.
                        this.Logger.Warning?.Log( $"Cannot find the source node for {FormatDiagnostic( diagnostic )}." );

                        continue;
                    }

                    Location newLocation;

                    // Find the token in the new syntax tree corresponding to the token in the old syntax tree.
                    var oldToken = oldNode.FindToken( diagnostic.Location.SourceSpan.Start );
                    var newToken = newNode.ChildTokens().SingleOrDefault( t => t.Text == oldToken.Text );

                    if ( newToken.IsKind( SyntaxKind.None ) )
                    {
                        // We could not find the old token in the new tree. This should not happen if cache invalidation is correct.
                        this.Logger.Warning?.Log( $"Cannot find the source token for {FormatDiagnostic( diagnostic )}." );

                        continue;
                    }

                    if ( newToken.Span.Length == diagnostic.Location.SourceSpan.Length )
                    {
                        // The diagnostic was reported to the exact token we found, so we can report it precisely.
                        newLocation = newToken.GetLocation();
                    }
                    else
                    {
                        // The diagnostic was reported on the syntax node we found, but not to an exact token. Report the
                        // diagnostic to the whole node instead.
                        newLocation = newNode.GetLocation();
                    }

                    var relocatedDiagnostic =
                        Diagnostic.Create(
                            designTimeDiagnostic.Id,
                            designTimeDiagnostic.Descriptor.Category,
                            new NonLocalizedString( diagnostic.GetMessage( CultureInfo.CurrentCulture ) ),
                            designTimeDiagnostic.Severity,
                            designTimeDiagnostic.DefaultSeverity,
                            true,
                            diagnostic.WarningLevel,
                            location: newLocation,
                            properties: designTimeDiagnostic.Properties );

                    this.Logger.Trace?.Log( $"Reporting {FormatDiagnostic( designTimeDiagnostic )}." );
                    reportDiagnostic( relocatedDiagnostic );
                }
            }
        }

        private static string FormatDiagnostic( Diagnostic d )
            => d.Properties.IsEmpty
                ? $"diagnostic `{d}`"
                : $"diagnostic `{d}` with properties " + string.Join( ", ", d.Properties.SelectAsEnumerable( p => $"'{p.Key}'='{p.Value}'" ) );
    }
}