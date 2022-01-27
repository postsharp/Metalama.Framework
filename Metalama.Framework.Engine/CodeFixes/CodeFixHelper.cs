// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes
{
    /// <summary>
    /// Helper methods used by different implementations of code fixes.
    /// </summary>
    public static class CodeFixHelper
    {
        public static async Task<Solution> ApplyChangesAsync(
            PartialCompilation compilation,
            Microsoft.CodeAnalysis.Project project,
            CancellationToken cancellationToken )
        {
            var solution = project.Solution;

            foreach ( var modifiedSyntaxTree in compilation.ModifiedSyntaxTrees )
            {
                var document = project.GetDocument( modifiedSyntaxTree.Value.OldTree );

                if ( document == null || !document.SupportsSyntaxTree )
                {
                    continue;
                }

                var newSyntaxRoot = await modifiedSyntaxTree.Value.NewTree.GetRootAsync( cancellationToken );

                var newDocument = document.WithSyntaxRoot( newSyntaxRoot );

                var formattedSyntaxRoot = await OutputCodeFormatter.FormatToSyntaxAsync(
                    newDocument,
                    reformatAll: false,
                    cancellationToken: cancellationToken );

                solution = solution.WithDocumentSyntaxRoot( document.Id, formattedSyntaxRoot );
            }

            return solution;
        }

        public static async Task<Solution> ReportErrorAsCommentsAsync(
            ISymbol targetSymbol,
            Document targetDocument,
            string error,
            CancellationToken cancellationToken )
        {
            var targetNode = await targetSymbol.GetPrimarySyntaxReference().AssertNotNull().GetSyntaxAsync( cancellationToken );

            var commentedNode = targetNode.WithLeadingTrivia(
                targetNode.GetLeadingTrivia().AddRange( new[] { SyntaxFactory.Comment( "// " + error ), SyntaxFactory.LineFeed } ) );

            var newSyntaxRoot = (await targetDocument.GetSyntaxRootAsync( cancellationToken ))!.ReplaceNode( targetNode, commentedNode );

            var newDocument = targetDocument.WithSyntaxRoot( newSyntaxRoot );

            return newDocument.Project.Solution;
        }

        public static async Task<Solution> ReportDiagnosticsAsCommentsAsync(
            ISymbol targetSymbol,
            Document targetDocument,
            ImmutableArray<Diagnostic> diagnostics,
            CancellationToken cancellationToken )
        {
            var targetNode = await targetSymbol.GetPrimarySyntaxReference().AssertNotNull().GetSyntaxAsync( cancellationToken );

            var commentedNode = targetNode.WithLeadingTrivia(
                targetNode.GetLeadingTrivia()
                    .Concat(
                        diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error )
                            .SelectMany(
                                d => new[] { SyntaxFactory.Comment( "// " + d.GetMessage( UserMessageFormatter.Instance ) ), SyntaxFactory.LineFeed } ) ) );

            var newSyntaxRoot = (await targetDocument.GetSyntaxRootAsync( cancellationToken ))!.ReplaceNode( targetNode, commentedNode );

            var newDocument = targetDocument.WithSyntaxRoot( newSyntaxRoot );

            return newDocument.Project.Solution;
        }
    }
}