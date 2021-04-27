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
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class DesignTimeDiagnosticSuppressor : DiagnosticSuppressor
    {
        private static readonly string[] _vs = { "CS1998", "IDE0051" };
        private static readonly string[] _supportedSuppressions = _vs;

        private static readonly ImmutableDictionary<string, SuppressionDescriptor> _supportedSuppressionsDictionary
            = ImmutableDictionary.Create<string, SuppressionDescriptor>( StringComparer.OrdinalIgnoreCase )
                .AddRange(
                    _supportedSuppressions.Select(
                        id => new KeyValuePair<string, SuppressionDescriptor>(
                            id,
                            new SuppressionDescriptor( "Caravela." + id, id, "Caravela" ) ) ) );

        public override void ReportSuppressions( SuppressionAnalysisContext context )
        {
            if ( CaravelaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            var syntaxTrees = context.ReportedDiagnostics
                .Select( d => d.Location.SourceTree )
                .WhereNotNull()
                .ToList();

            ReportSuppressions(
                compilation,
                syntaxTrees,
                context.ReportedDiagnostics,
                context.ReportSuppression,
                new BuildOptions( context.Options.AnalyzerConfigOptionsProvider ),
                context.CancellationToken );
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

            foreach ( var syntaxTreeResult in results.SyntaxTreeResults )
            {
                if ( syntaxTreeResult == null )
                {
                    continue;
                }

                // Report suppressions.
                if ( !syntaxTreeResult.Suppressions.IsDefaultOrEmpty )
                {
                    var designTimeSuppressions = syntaxTreeResult.Suppressions.Where( s => _supportedSuppressions.Contains( s.Id ) );

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

                        if ( groupedSuppressions[symbolId].Any( s => string.Equals( s.Id, diagnostic.Id, StringComparison.OrdinalIgnoreCase ) ) )
                        {
                            reportSuppression( Suppression.Create( _supportedSuppressionsDictionary[diagnostic.Id], diagnostic ) );
                        }
                    }
                }
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => _supportedSuppressionsDictionary.Values.ToImmutableArray();
    }
}