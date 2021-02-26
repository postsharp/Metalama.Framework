using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.TestFramework
{
    public class TriviaAdder : CSharpSyntaxRewriter
    {
        //TODO: resolve abmiguity [return: NotNullIfNotNullAttribute( "node" )]
        public override SyntaxNode? Visit( SyntaxNode? node )
        {
            var newNode = base.Visit( node );

            if ( newNode == null )
            {
                return null;
            }

            //TODO: preserve original trivias
            newNode = newNode.WithLeadingTrivia( SyntaxFactory.Space );
            newNode = newNode.WithTrailingTrivia( SyntaxFactory.Space );
            return newNode;
        }
    }
}
