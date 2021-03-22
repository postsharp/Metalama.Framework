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
