// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
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
        public static async Task<Solution> ReportErrorsAsCommentsAsync(
            SyntaxNode targetNode,
            Document targetDocument,
            IEnumerable<string> errors,
            CancellationToken cancellationToken )
        {
            var commentedNode = targetNode.WithLeadingTrivia(
                targetNode.GetLeadingTrivia().Concat( errors.SelectMany( e => new[] { SyntaxFactory.Comment( "// " + e ), SyntaxFactory.LineFeed } ) ) );

            var newSyntaxRoot = (await targetDocument.GetSyntaxRootAsync( cancellationToken ))!.ReplaceNode( targetNode, commentedNode );

            var newDocument = targetDocument.WithSyntaxRoot( newSyntaxRoot );

            return newDocument.Project.Solution;
        }

        public static string GetDiagnosticMessage( Diagnostic diagnostic ) => diagnostic.GetMessage( UserMessageFormatter.Instance );
    }
}