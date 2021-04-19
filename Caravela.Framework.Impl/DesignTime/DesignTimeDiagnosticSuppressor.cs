// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
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
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class DesignTimeDiagnosticSuppressor : DiagnosticSuppressor
    {
        private static readonly string[] _vs = { "IDE001" };
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

            ReportSuppressions(
                compilation,
                context.ReportedDiagnostics,
                context.ReportSuppression,
                new BuildOptions( new AnalyzerBuildOptionsSource( context.Options.AnalyzerConfigOptionsProvider ) ),
                context.CancellationToken );
        }

        /// <summary>
        /// A testable overload of <see cref="ReportSuppressions(SuppressionAnalysisContext)"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="reportedDiagnostics"></param>
        /// <param name="reportSuppression"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        internal static void ReportSuppressions(
            CSharpCompilation compilation,
            ImmutableArray<Diagnostic> reportedDiagnostics,
            Action<Suppression> reportSuppression,
            BuildOptions options,
            CancellationToken cancellationToken )
        {
            // Execute the pipeline.
            var pipelineResult = DesignTimeAspectPipelineCache.GetPipelineResult(
                compilation,
                options,
                cancellationToken );

            // Report suppressions.
            if ( !pipelineResult.Diagnostics.DiagnosticSuppressions.IsDefaultOrEmpty )
            {
                var designTimeSuppressions = pipelineResult.Diagnostics.DiagnosticSuppressions.Where(
                    s => _supportedSuppressions.Contains( s.Id ) && ((ISdkCodeElement) s.CodeElement).Symbol != null );

                var groupedSuppressions = ImmutableMultiValueDictionary<ISymbol, ScopedSuppression>.Create(
                    designTimeSuppressions,
                    s => ((ISdkCodeElement) s.CodeElement).Symbol! );

                foreach ( var diagnostic in reportedDiagnostics )
                {
                    if ( diagnostic.Location.SourceTree == null )
                    {
                        continue;
                    }

                    // TODO: address warning.
#pragma warning disable RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
                    var semanticModel = compilation.GetSemanticModel( diagnostic.Location.SourceTree );
#pragma warning restore RS1030 // Do not invoke Compilation.GetSemanticModel() method within a diagnostic analyzer
                    var symbol = semanticModel.GetEnclosingSymbol( diagnostic.Location.SourceSpan.Start );

                    if ( symbol == null )
                    {
                        continue;
                    }

                    if ( groupedSuppressions[symbol].Any( s => string.Equals( s.Id, diagnostic.Id, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        reportSuppression( Suppression.Create( _supportedSuppressionsDictionary[diagnostic.Id], diagnostic ) );
                    }
                }
            }
        }

        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => _supportedSuppressionsDictionary.Values.ToImmutableArray();
    }
}