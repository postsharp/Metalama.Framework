using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating
{

    public sealed partial class TextSpanClassifier
    {
        private class MarkAllChildrenWalker : CSharpSyntaxWalker
        {
            private readonly TextSpanClassifier _parent;
            private TextSpanClassification _classification;

            public MarkAllChildrenWalker( TextSpanClassifier parent ) : base( SyntaxWalkerDepth.StructuredTrivia )
            {
                this._parent = parent;
            }

            public void MarkAll( SyntaxNode node, TextSpanClassification classification )
            {
                this._classification = classification;
                this.Visit( node );
            }

            public override void DefaultVisit( SyntaxNode node )
            {
                foreach ( var child in node.ChildNodesAndTokens() )
                {
                    if ( child.IsNode )
                    {
                        this.Visit( child.AsNode() );
                    }
                    else
                    {
                        this._parent.Mark( child.AsToken(), this._classification );
                    }
                }

                if ( ShouldMarkTrivias( this._classification ) )
                {

                    this._parent.Mark( node.GetLeadingTrivia(), this._classification );
                    this._parent.Mark( node.GetTrailingTrivia(), this._classification );
                }
                else
                {
                    // We don't highlight the trivia of "special" spans because they are typically keyword-like.
                }
            }
        }
    }
}