// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.AspectWorkbench.ViewModels
{
    internal class AspectReferenceRenderingRewriter : CSharpSyntaxRewriter
    {
        [return: NotNullIfNotNull( "node" )]
        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            var transformedNode = base.Visit( node );

            if ( node != null && transformedNode != null && node.TryGetAspectReference( out var aspectReference ) )
            {
                // ReSharper disable once StringLiteralTypo
                return transformedNode
                    .WithLeadingTrivia(
                        transformedNode.GetLeadingTrivia()
                            .Add(
                                SyntaxTrivia(
                                    SyntaxKind.MultiLineCommentTrivia,
                                    $"/*REF({aspectReference.ToString( true )})*/" ) ) )
                    .WithTrailingTrivia(
                        transformedNode.GetTrailingTrivia()
                            .Insert(
                                0,
                                SyntaxTrivia(
                                    SyntaxKind.MultiLineCommentTrivia,
                                    "/*ENDREF*/" ) ) );
            }
            else
            {
                return transformedNode;
            }
        }
    }
}