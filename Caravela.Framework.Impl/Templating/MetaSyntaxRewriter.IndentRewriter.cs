using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal abstract partial class MetaSyntaxRewriter
    {
        private class IndentRewriter : CSharpSyntaxRewriter
        {
            private readonly MetaSyntaxRewriter _parent;
            private const int limit = 80;

            public IndentRewriter( MetaSyntaxRewriter parent )
            {
                this._parent = parent;
            }

            public override SyntaxNode? VisitArgumentList( ArgumentListSyntax node )
            {
                if ( node.Arguments.Count > 1 || node.Arguments.Span.Length > limit )
                {
                    this._parent.Indent();
                    try
                    {
                        var indentedArguments =
                            node.Arguments.Select( a => this.Visit( a )!.WithLeadingTrivia( this._parent.GetIndentation() ) );

                        return SyntaxFactory.ArgumentList(
                            node.OpenParenToken.WithTrailingTrivia( this._parent.GetIndentation( false ) ),
                            SyntaxFactory.SeparatedList( indentedArguments ), node.CloseParenToken );
                    }
                    finally
                    {
                        this._parent.Unindent();
                    }
                }
                else
                {
                    return base.VisitArgumentList( node );
                }
            }

            public override SyntaxNode? VisitInitializerExpression( InitializerExpressionSyntax node )
            {
                if ( node.Expressions.Count > 1 || node.Span.Length > limit )
                {
                    this._parent.Indent();
                    try
                    {
                        var indentedExpressions =
                            node.Expressions.Select( a => this.Visit( a )!.WithLeadingTrivia( this._parent.GetIndentation() ) );

                        return SyntaxFactory.InitializerExpression(
                            node.Kind(),
                            node.OpenBraceToken.WithTrailingTrivia( this._parent.GetIndentation( false ) ),
                            SyntaxFactory.SeparatedList( indentedExpressions ), node.CloseBraceToken );
                    }
                    finally
                    {
                        this._parent.Unindent();
                    }
                }
                else
                {
                    return base.VisitInitializerExpression( node );
                }
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node == null || node.Span.Length < limit || node.HasNoDeepIndentAnnotation() )
                {
                    return node;
                }
                else
                {
                    return base.Visit( node );
                }
            }
        }
    }
}