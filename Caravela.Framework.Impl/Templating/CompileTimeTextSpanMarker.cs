using System;
using System.Collections.Immutable;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Produces a <see cref="MarkedSpanSet"/> with compile-time code given
    /// a syntax tree annotated with <see cref="TemplateAnnotator"/>.
    /// </summary>
    public sealed class CompileTimeTextSpanMarker : CSharpSyntaxWalker
    {
        private readonly MarkedSpanSet _spans = new MarkedSpanSet();
        private readonly SourceText _sourceText;

        public CompileTimeTextSpanMarker( SourceText sourceText )
        {
            this._sourceText = sourceText;
        }

        public override void DefaultVisit( SyntaxNode? node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( node );
            }
            else
            {
                base.DefaultVisit( node );
            }
        }

        private void Mark( SyntaxNode node )
        {
            if ( node != null )
            {
                this.Mark( node.Span );
            }
        }

        private void Mark( SyntaxToken token ) => this.Mark( token.Span );


        private void Mark( TextSpan span )
        {
            if ( span.IsEmpty )
                return;

#if DEBUG

            var text = this._sourceText.GetSubText( span ).ToString();
#endif

            this._spans.Mark( span );
        }

        public ImmutableList<TextSpan> GetMarkedSpans() => this._spans.GetMarkedSpans();


        public override void VisitIfStatement( IfStatementSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( node.IfKeyword );
                this.Mark( node.Condition );
                this.Mark( node.OpenParenToken );
                this.Mark( node.CloseParenToken );
                this.VisitCompileTimeStatementNode( node.Statement );

                if ( node.Else != null )
                {
                    this.Mark( node.Else.ElseKeyword );
                    this.VisitCompileTimeStatementNode( node.Else.Statement );
                }
            }
            else
            {
                base.VisitIfStatement( node );
            }
        }

        public override void VisitForEachStatement( ForEachStatementSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( node.ForEachKeyword );
                this.Mark( node.Type );
                this.Mark( node.Identifier );
                this.Mark( node.Expression );
                this.Mark( node.OpenParenToken );
                this.Mark( node.CloseParenToken );
                this.VisitCompileTimeStatementNode( node.Statement );
            }
            else
            {
                base.VisitForEachStatement( node );
            }
        }

        private void VisitCompileTimeStatementNode( StatementSyntax statement )
        {
            if ( statement is BlockSyntax block )
            {
                this.Mark( block.OpenBraceToken );
                this.Mark( block.CloseBraceToken );
            }

            this.Visit( statement );
        }
    }
}