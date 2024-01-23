﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Services;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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
    public class TheDiagnosticAnalyzer : DefinitionOnlyDiagnosticAnalyzer
    {
        private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
        private readonly ILogger _logger;

        [UsedImplicitly]
        public TheDiagnosticAnalyzer() : this(
            DesignTimeServiceProviderFactory.GetSharedServiceProvider<DesignTimeAnalysisProcessServiceProviderFactory>() ) { }

        public TheDiagnosticAnalyzer( GlobalServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
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

        internal void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
            => this.AnalyzeSemanticModel( new SemanticModelAnalysisContextAdapter( context ) );

        internal void AnalyzeSemanticModel( ISemanticModelAnalysisContext context )
        {
            try
            {
                var compilation = context.SemanticModel.Compilation;

                var syntaxTreeFilePath = context.SemanticModel.SyntaxTree.FilePath;

                this._logger.Trace?.Log(
                    $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}) started." );

                var projectOptions = context.ProjectOptions;

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    this._logger.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

                    return;
                }

                // Execute the pipeline.
                var cancellationToken = context.CancellationToken.IgnoreIfDebugging().ToTestable();

                var pipeline = this._pipelineFactory.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

                if ( pipeline == null )
                {
                    return;
                }

                var diagnosticCount = 0;

                var reportedDiagnostics = new HashSet<Diagnostic>();

                void ReportDiagnostic( Diagnostic diagnostic )
                {
                    // TemplateAnnotator is run from both TemplatingCodeValidator and from the pipeline, so its diagnostics are reported twice.
                    if ( !reportedDiagnostics.Add( diagnostic ) )
                    {
                        this._logger.Trace?.Log( $"Not reporting {FormatDiagnostic( diagnostic )} again." );

                        return;
                    }

                    this._logger.Trace?.Log( $"Reporting {FormatDiagnostic( diagnostic )}." );

                    diagnosticCount++;

                    context.ReportDiagnostic( diagnostic );
                }

                // Run the pipeline.
                IEnumerable<Diagnostic> diagnostics;
                IEnumerable<IScopedSuppression> suppressions;

                var pipelineResult = pipeline.Execute( compilation, cancellationToken );

                var filteredPipelineDiagnostics = pipelineResult.Diagnostics.Where( d => d.Location.SourceTree?.FilePath == syntaxTreeFilePath );

                if ( !pipelineResult.IsSuccessful )
                {
                    this._logger.Trace?.Log(
                        $"DesignTimeAnalyzer.AnalyzeSemanticModel('{syntaxTreeFilePath}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed. It returned {pipelineResult.Diagnostics.Length} diagnostics." );

                    diagnostics = filteredPipelineDiagnostics;
                    suppressions = Enumerable.Empty<IScopedSuppression>();
                }
                else
                {
                    diagnostics = pipelineResult.Value.GetAllDiagnostics( syntaxTreeFilePath ).Concat( filteredPipelineDiagnostics );
                    suppressions = pipelineResult.Value.GetSuppressionOnSyntaxTree( syntaxTreeFilePath );
                }

                // Execute the analyses that are not performed in the pipeline.
                // We execute this after the pipeline so we allow it to get to paused state in case of compile-time change.
                TemplatingCodeValidator.Validate(
                    pipeline.ServiceProvider,
                    context.SemanticModel,
                    ReportDiagnostic,
                    pipeline.MustReportPausedPipelineAsErrors && pipeline.IsCompileTimeSyntaxTreeOutdated( context.SemanticModel.SyntaxTree.FilePath ),
                    true,
                    cancellationToken );

                // Execute the reference validators.
                if ( pipelineResult.IsSuccessful )
                {
                    var validationResults = DesignTimeReferenceValidatorRunner.Validate(
                        pipelineResult.Value.Configuration.ServiceProvider,
                        context.SemanticModel,
                        pipelineResult.Value,
                        context.CancellationToken );

                    diagnostics = diagnostics.Concat( validationResults.ReportedDiagnostics );
                    suppressions = suppressions.Concat( validationResults.DiagnosticSuppressions );
                }

                // Report diagnostics.
                this.ReportDiagnostics(
                    diagnostics,
                    compilation,
                    ReportDiagnostic,
                    cancellationToken );

                // If we have unsupported suppressions, a diagnostic here because a Suppressor cannot report.
                foreach ( var suppression in suppressions.Where(
                             s => !this.DiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) ) )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var symbol = suppression.GetScopeSymbolOrNull( compilation );

                    if ( symbol != null )
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

                this._logger.Trace?.Log(
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
        private void ReportDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            Compilation compilation,
            Action<Diagnostic> reportDiagnostic,
            CancellationToken cancellationToken )
        {
            foreach ( var diagnostic in diagnostics )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( diagnostic.Id.StartsWith( "CS", StringComparison.OrdinalIgnoreCase ) )
                {
                    // Do not try to report C# errors. They are sent by the template compiler and are useful in other UI parts
                    // but they should not be reported by the analyzer.
                    continue;
                }

                Diagnostic designTimeDiagnostic;

                if ( !this.ShouldWrapUnsupportedDiagnostics || this.DiagnosticDefinitions.SupportedDiagnosticDescriptors.ContainsKey( diagnostic.Id ) )
                {
                    designTimeDiagnostic = diagnostic;
                }
                else
                {
                    this._logger.Warning?.Log( $"The diagnostic {diagnostic.Id} is not supported at design time. Wrapping it." );

                    var descriptor =
                        diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => DesignTimeDiagnosticDescriptors.UserError,
                            DiagnosticSeverity.Hidden => DesignTimeDiagnosticDescriptors.UserHidden,
                            DiagnosticSeverity.Warning => DesignTimeDiagnosticDescriptors.UserWarning,
                            DiagnosticSeverity.Info => DesignTimeDiagnosticDescriptors.UserInfo,
                            _ => throw new AssertionFailedException( $"Unexpected severity: {diagnostic.Severity}." )
                        };

                    designTimeDiagnostic = descriptor.CreateRoslynDiagnostic(
                        diagnostic.Location,
                        (diagnostic.Id, diagnostic.GetLocalizedMessage()),
                        properties: diagnostic.Properties );
                }

                var wasAnyLocationRemapped = false;

                // Map the diagnostic locations to the compilation.
                if ( !this.TryMapLocation( diagnostic.Location, compilation, out var mappedLocation, ref wasAnyLocationRemapped ) )
                {
                    this._logger.Warning?.Log( $"Cannot report {diagnostic}." );

                    continue;
                }

                IReadOnlyList<Location> mappedAdditionalLocations;

                if ( diagnostic.AdditionalLocations.Any( d => d.SourceTree != null && !compilation.ContainsSyntaxTree( d.SourceTree ) ) )
                {
                    var mappedAdditionalLocationsList = new List<Location>( diagnostic.AdditionalLocations.Count );
                    mappedAdditionalLocations = mappedAdditionalLocationsList;

                    foreach ( var t in diagnostic.AdditionalLocations )
                    {
                        if ( !this.TryMapLocation(
                                t,
                                compilation,
                                out var mappedAdditionalLocation,
                                ref wasAnyLocationRemapped ) )
                        {
                            // If we can't mapped an additional location, we just skip the location
                            // but not the whole diagnostic.
                            continue;
                        }

                        mappedAdditionalLocationsList.Add( mappedAdditionalLocation );
                    }
                }
                else
                {
                    mappedAdditionalLocations = diagnostic.AdditionalLocations;
                }

                this._logger.Trace?.Log( $"Reporting {FormatDiagnostic( designTimeDiagnostic )}." );

                if ( wasAnyLocationRemapped )
                {
                    var relocatedDiagnostic =
                        Diagnostic.Create(
                            designTimeDiagnostic.Id,
                            designTimeDiagnostic.Descriptor.Category,
                            new NonLocalizedString( designTimeDiagnostic.GetLocalizedMessage() ),
                            designTimeDiagnostic.Severity,
                            designTimeDiagnostic.DefaultSeverity,
                            isEnabledByDefault: true,
                            designTimeDiagnostic.WarningLevel,
                            designTimeDiagnostic.Descriptor.Title,
                            location: mappedLocation,
                            properties: designTimeDiagnostic.Properties,
                            additionalLocations: mappedAdditionalLocations );

                    reportDiagnostic( relocatedDiagnostic );
                }
                else
                {
                    reportDiagnostic( designTimeDiagnostic );
                }
            }
        }

        private bool TryMapLocation( Location location, Compilation compilation, [NotNullWhen( true )] out Location? mappedLocation, ref bool hasChange )
        {
            var reportSourceTree = location.SourceTree;

            if ( reportSourceTree == null || compilation.ContainsSyntaxTree( reportSourceTree ) )
            {
                mappedLocation = location;

                return true;
            }
            else
            {
                hasChange = true;

                this._logger.Trace?.Log( "Finding the source tree." );

                // Find the new syntax tree in the compilation.

                if ( !compilation.GetIndexedSyntaxTrees().TryGetValue( reportSourceTree.FilePath, out var newSyntaxTree ) )
                {
                    mappedLocation = Location.Create( reportSourceTree.FilePath, location.SourceSpan, location.GetLineSpan().Span );

                    return true;
                }

                // Find the node in the new syntax tree corresponding to the node in the old syntax tree.
                var oldNode = reportSourceTree.GetRoot().FindNode( location.SourceSpan, getInnermostNodeForTie: true );

                if ( !NodeFinder.TryFindOldNodeInNewTree( oldNode, newSyntaxTree, out var newNode ) )
                {
                    // We could not find the old node in the new tree. This should not happen if cache invalidation is correct.
                    this._logger.Warning?.Log( $"Cannot find the source node for location {location}." );

                    mappedLocation = null;

                    return false;
                }

                // Find the token in the new syntax tree corresponding to the token in the old syntax tree.
                var oldToken = oldNode.FindToken( location.SourceSpan.Start );
                var newToken = newNode.ChildTokens().SingleOrDefault( t => t.Text == oldToken.Text );

                if ( newToken.IsKind( SyntaxKind.None ) )
                {
                    // We could not find the old token in the new tree. This should not happen if cache invalidation is correct.
                    this._logger.Warning?.Log( $"Cannot find the source token for location {location}." );

                    mappedLocation = null;

                    return false;
                }

                if ( newToken.Span.Length == location.SourceSpan.Length )
                {
                    // The diagnostic was reported to the exact token we found, so we can report it precisely.
                    mappedLocation = newToken.GetLocation();
                }
                else
                {
                    // The diagnostic was reported on the syntax node we found, but not to an exact token. Report the
                    // diagnostic to the whole node instead.
                    mappedLocation = newNode.GetLocation();
                }

                return true;
            }
        }

        private static string FormatDiagnostic( Diagnostic d )
            => d.Properties.IsEmpty
                ? $"diagnostic `{d}`"
                : $"diagnostic `{d}` with properties " + string.Join( ", ", d.Properties.SelectAsReadOnlyCollection( p => $"'{p.Key}'='{p.Value}'" ) );
    }
}