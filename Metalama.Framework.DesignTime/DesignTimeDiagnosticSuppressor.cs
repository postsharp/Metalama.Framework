// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.DesignTime.Diagnostics;
using Metalama.Framework.Engine.DesignTime.Pipeline;
using Metalama.Framework.Engine.DesignTime.Utilities;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#pragma warning disable RS1022 // Remove access to our implementation types.

namespace Metalama.Framework.Engine.DesignTime
{
    // ReSharper disable UnusedType.Global

    /// <summary>
    /// Our implementation of <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DesignTimeDiagnosticSuppressor : DiagnosticSuppressor
    {
        static DesignTimeDiagnosticSuppressor()
        {
            Logger.Initialize();
        }

        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        public override void ReportSuppressions( SuppressionAnalysisContext context )
        {
            if ( MetalamaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                Logger.DesignTime.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{context.Compilation.AssemblyName}')." );

                var buildOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                if ( !buildOptions.IsDesignTimeEnabled )
                {
                    Logger.DesignTime.Trace?.Log( $"DesignTimeAnalyzer.AnalyzeSemanticModel: design time experience is disabled." );

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
            ProjectOptions options,
            CancellationToken cancellationToken )
        {
            // Execute the pipeline.
            if ( !DesignTimeAspectPipelineFactory.Instance.TryExecute(
                    options,
                    compilation,
                    cancellationToken,
                    out var compilationResult ) )
            {
                Logger.DesignTime.Trace?.Log( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): the pipeline failed." );

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

                var suppressions = compilationResult.GetDiagnosticsOnSyntaxTree( syntaxTree.FilePath ).Suppressions;

                var designTimeSuppressions = suppressions.Where(
                        s => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) )
                    .ToList();

                if ( designTimeSuppressions.Count == 0 )
                {
                    continue;
                }

#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
                var semanticModel = compilation.GetSemanticModel( syntaxTree );
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer

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

            Logger.DesignTime.Trace?.Log(
                $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): {suppressionsCount} suppressions reported." );
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}