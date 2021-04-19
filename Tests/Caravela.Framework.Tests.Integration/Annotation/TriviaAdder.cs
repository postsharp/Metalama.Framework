// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Tests.Integration.Annotation
{
    /// <summary>
    /// Adds a space before and after each node.
    /// </summary>
    public class TriviaAdder : CSharpSyntaxRewriter
    {
        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            var newNode = base.Visit( node );

            if ( newNode == null )
            {
                return null;
            }

            newNode = newNode.WithLeadingTrivia( SyntaxFactory.Space );
            newNode = newNode.WithTrailingTrivia( SyntaxFactory.Space );

            return newNode;
        }
    }
}