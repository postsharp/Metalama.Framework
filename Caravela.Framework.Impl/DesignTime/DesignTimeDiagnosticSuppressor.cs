// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#pragma warning disable RS1022 // Remove access to our implementation types.

namespace Caravela.Framework.Impl.DesignTime
{
    // ReSharper disable UnusedType.Global

    /// <summary>
    /// Our implementation of <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DesignTimeDiagnosticSuppressor : DiagnosticSuppressor
    {
        private readonly DesignTimeDiagnosticDefinitions _designTimeDiagnosticDefinitions = DesignTimeDiagnosticDefinitions.GetInstance();

        public override void ReportSuppressions( SuppressionAnalysisContext context )
        {
            if ( CaravelaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                Logger.Instance?.Write( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{context.Compilation.AssemblyName}')." );

                var syntaxTrees = context.ReportedDiagnostics
                    .Select( d => d.Location.SourceTree )
                    .WhereNotNull()
                    .ToList();

                var buildOptions = new ProjectOptions( context.Options.AnalyzerConfigOptionsProvider );

                DebuggingHelper.AttachDebugger( buildOptions );

                this.ReportSuppressions(
                    compilation,
                    syntaxTrees,
                    context.ReportedDiagnostics,
                    context.ReportSuppression,
                    buildOptions,
                    context.CancellationToken );
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
            IReadOnlyList<SyntaxTree> syntaxTrees,
            ImmutableArray<Diagnostic> reportedDiagnostics,
            Action<Suppression> reportSuppression,
            ProjectOptions options,
            CancellationToken cancellationToken )
        {
            // Execute the pipeline.
            var results = DesignTimeAspectPipelineCache.Instance.GetSyntaxTreeResults(
                compilation,
                syntaxTrees,
                options,
                cancellationToken );

            var suppressionsCount = 0;

            foreach ( var syntaxTreeResult in results )
            {
                if ( syntaxTreeResult == null )
                {
                    continue;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Report suppressions.
                if ( !syntaxTreeResult.Suppressions.IsDefaultOrEmpty )
                {
                    var designTimeSuppressions = syntaxTreeResult.Suppressions.Where(
                        s => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.ContainsKey( s.Definition.SuppressedDiagnosticId ) );

                    var groupedSuppressions = ImmutableMultiValueDictionary<string, CacheableScopedSuppression>.Create(
                        designTimeSuppressions,
                        s => s.SymbolId );

                    foreach ( var diagnostic in reportedDiagnostics )
                    {
                        if ( diagnostic.Location.SourceTree == null )
                        {
                            continue;
                        }

#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
                        var semanticModel = compilation.GetSemanticModel( diagnostic.Location.SourceTree );
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer

                        var diagnosticNode = diagnostic.Location.SourceTree.GetRoot().FindNode( diagnostic.Location.SourceSpan );
                        var symbol = semanticModel.GetDeclaredSymbol( diagnosticNode );

                        if ( symbol == null )
                        {
                            continue;
                        }

                        var symbolId = symbol.GetDocumentationCommentId().AssertNotNull();

                        foreach ( var suppression in groupedSuppressions[symbolId]
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
                                // We can't report a warning here, but DesignTimeAnalyzer should.
                            }
                        }
                    }
                }

                Logger.Instance?.Write(
                    $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): {suppressionsCount} suppressions reported." );
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => this._designTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}