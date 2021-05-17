// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="DiagnosticSuppressor"/>.
    /// </summary>
    public class DesignTimeDiagnosticSuppressor : DiagnosticSuppressor
    {
        public override void ReportSuppressions( SuppressionAnalysisContext context )
        {
            if ( CaravelaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            try
            {
                DesignTimeLogger.Instance?.Write( $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{context.Compilation.AssemblyName}')." );

                var syntaxTrees = context.ReportedDiagnostics
                    .Select( d => d.Location.SourceTree )
                    .WhereNotNull()
                    .ToList();

                var buildOptions = new BuildOptions( context.Options.AnalyzerConfigOptionsProvider );

                DesignTimeDebugger.AttachDebugger( buildOptions );

                ReportSuppressions(
                    compilation,
                    syntaxTrees,
                    context.ReportedDiagnostics,
                    context.ReportSuppression,
                    buildOptions,
                    context.CancellationToken );
            }
            catch ( Exception e )
            {
                DesignTimeLogger.Instance?.Write( e.ToString() );
            }
        }

        /// <summary>
        /// A testable overload of <see cref="ReportSuppressions(SuppressionAnalysisContext)"/>.
        /// </summary>
        private static void ReportSuppressions(
            Compilation compilation,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            ImmutableArray<Diagnostic> reportedDiagnostics,
            Action<Suppression> reportSuppression,
            BuildOptions options,
            CancellationToken cancellationToken )
        {
            // Execute the pipeline.
            var results = DesignTimeAspectPipelineCache.Instance.GetDesignTimeResults(
                compilation,
                syntaxTrees,
                options,
                cancellationToken );

            var suppressionsCount = 0;

            foreach ( var syntaxTreeResult in results.SyntaxTreeResults )
            {
                if ( syntaxTreeResult == null )
                {
                    continue;
                }

                // Report suppressions.
                if ( !syntaxTreeResult.Suppressions.IsDefaultOrEmpty )
                {
                    var designTimeSuppressions = syntaxTreeResult.Suppressions.Where(
                        s => DesignTimeDiagnosticDefinitions.IsSuppressionSupported( s.Definition.SuppressedDiagnosticId ) );

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

                            if ( DesignTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.TryGetValue(
                                suppression.Definition.Id,
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

                DesignTimeLogger.Instance?.Write(
                    $"DesignTimeDiagnosticSuppressor.ReportSuppressions('{compilation.AssemblyName}'): {suppressionsCount} suppressions reported." );
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions
            => DesignTimeDiagnosticDefinitions.SupportedSuppressionDescriptors.Values.ToImmutableArray();
    }
}