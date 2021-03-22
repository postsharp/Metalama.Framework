using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.TestFramework
{
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
