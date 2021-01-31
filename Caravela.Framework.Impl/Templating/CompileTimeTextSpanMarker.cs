using Caravela.Framework.DesignTime.Contracts;
using System;
using System.Collections.Immutable;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Produces a <see cref="TextSpanClassifier"/> with compile-time code given
    /// a syntax tree annotated with <see cref="TemplateAnnotator"/>.
    /// </summary>
    public sealed class CompileTimeTextSpanMarker : CSharpSyntaxWalker
    {
        private readonly TextSpanClassifier _textSpans = new TextSpanClassifier();
        private readonly SourceText _sourceText;

        private bool _isInTemplate;

        public CompileTimeTextSpanMarker( SourceText sourceText )
        {
            this._sourceText = sourceText;
        }

        public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.Template )
            {
                this._isInTemplate = true;

                base.VisitMethodDeclaration( node );

                this._isInTemplate = false;
            }
        }

        public override void VisitToken( SyntaxToken token )
        {
            if ( this._isInTemplate )
            {
                var colorFromAnnotation = token.GetColorFromAnnotation();
                if ( colorFromAnnotation != TextSpanCategory.Default )
                {
                    this.Mark( token, colorFromAnnotation );
                }
            }

        }

        public override void DefaultVisit( SyntaxNode node )
        {
            if ( this._isInTemplate )
            {
                if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
                {
                    // This could be overwritten later.
                    this.Mark( node, TextSpanCategory.CompileTime );
                }


                var colorFromAnnotation = node.GetColorFromAnnotation();
                if ( colorFromAnnotation != TextSpanCategory.Default )
                {
                    this.Mark( node, colorFromAnnotation );
                }
            }

            base.DefaultVisit( node );
        }

        private void Mark( SyntaxNode node , TextSpanCategory category) => this.Mark( node.Span, category );

        private void Mark( SyntaxToken token, TextSpanCategory category ) => this.Mark( token.Span, category );


        private void Mark( TextSpan span, TextSpanCategory category )
        {
            if ( span.IsEmpty )
                return;

#if DEBUG

            var text = this._sourceText.GetSubText( span ).ToString();
#endif

            this._textSpans.Mark( span, category );
        }

        public ITextSpanClassifier Classifier => this._textSpans;

        public override void VisitLiteralExpression( LiteralExpressionSyntax node )
        {
            // We don't mark literals that are not a part of larger compile-time expressions because it does not bring anything useful.
        }

        public override void VisitIfStatement( IfStatementSyntax node )
        {
            if ( this._isInTemplate && node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( TextSpan.FromBounds( node.IfKeyword.SpanStart, node.CloseParenToken.Span.End ), TextSpanCategory.CompileTime );
                this.Visit( node.Condition );
                this.VisitCompileTimeStatementNode( node.Statement );

                if ( node.Else != null )
                {
                    this.Mark( node.Else.ElseKeyword, TextSpanCategory.CompileTime );
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
            if ( this._isInTemplate && node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( TextSpan.FromBounds( node.ForEachKeyword.SpanStart, node.CloseParenToken.Span.End ), TextSpanCategory.CompileTime );
                this.Mark( node.Identifier, TextSpanCategory.Variable );
                this.Visit( node.Expression );
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
                this.Mark( block.OpenBraceToken, TextSpanCategory.CompileTime );
                this.Mark( block.CloseBraceToken, TextSpanCategory.CompileTime );
            }

            this.Visit( statement );
        }
    }
}