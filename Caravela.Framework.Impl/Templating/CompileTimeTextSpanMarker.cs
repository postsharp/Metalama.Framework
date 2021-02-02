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
    // TODO: This class is essentially a UI concern so it should be moved into a different assembly e.g. DesignTime.

    /// <summary>
    /// Produces a <see cref="TextSpanClassifier"/> with compile-time code given
    /// a syntax tree annotated with <see cref="TemplateAnnotator"/>.
    /// </summary>
    public sealed class CompileTimeTextSpanMarker : CSharpSyntaxWalker
    {
        private readonly TextSpanClassifier _textSpans = new TextSpanClassifier();
        private readonly SourceText _sourceText;
        private readonly string _sourceString;

        private bool _isInTemplate;
        
        public CompileTimeTextSpanMarker( SourceText sourceText)
        {
            this._sourceText = sourceText;
            this._sourceString = sourceText.ToString();
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( node.Modifiers, TextSpanCategory.CompileTime, true );
                this.Mark( node.Keyword, TextSpanCategory.CompileTime, true );
                this.Mark( node.OpenBraceToken, TextSpanCategory.CompileTime);
                this.Mark( node.CloseBraceToken, TextSpanCategory.CompileTime);
                this.Mark( node.ConstraintClauses, TextSpanCategory.CompileTime, true );
                this.Mark( node.Identifier, TextSpanCategory.CompileTime, true );
                this.Mark( node.BaseList, TextSpanCategory.CompileTime, true );
                base.VisitClassDeclaration( node );
            }
            else
            {
                // We don't process the type at all.
            }
        }

       
        public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.Template )
            {
                this._isInTemplate = true;

                this.Mark( node.ReturnType, TextSpanCategory.CompileTime, true );
                this.Mark( node.Identifier, TextSpanCategory.CompileTime, true );
                this.Mark( node.ParameterList, TextSpanCategory.CompileTime, true );
                this.Mark( node.Modifiers, TextSpanCategory.CompileTime, true );
                if ( node.Body != null )
                {
                    this.Mark( node.Body.OpenBraceToken, TextSpanCategory.CompileTime );
                    this.Mark( node.Body.CloseBraceToken, TextSpanCategory.CompileTime );
                }
                

                // The code is run-time by default in a template method.
                this.Mark( node.Body, TextSpanCategory.RunTime );
                this.Mark( node.ExpressionBody, TextSpanCategory.RunTime );

                base.VisitMethodDeclaration( node );

                this._isInTemplate = false;
            }
            else
            {
                this.Mark( node, TextSpanCategory.CompileTime );
            }
        }
                

        private void VisitMember( SyntaxNode node )
        {
            this.Mark( node, TextSpanCategory.CompileTime );
        }

        public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
        {
            this.VisitMember( node );
        }

        public override void VisitEventDeclaration( EventDeclarationSyntax node )
        {
            this.VisitMember( node );
        }

        public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            this.VisitMember( node );
        }

        public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
        {
            this.VisitMember( node );
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

        public override void VisitVariableDeclarator( VariableDeclaratorSyntax node )
        {
            base.VisitVariableDeclarator( node );

            var colorFromAnnotation = node.Identifier.GetColorFromAnnotation();
            if ( colorFromAnnotation != TextSpanCategory.Default )
            {
                this.Mark( node.Identifier, colorFromAnnotation );
            }
        }

        public override void DefaultVisit( SyntaxNode node )
        {
            if ( this._isInTemplate )
            {
                if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
                {
                    // This can be overwritten later in a child node.
                    this.Mark( node, TextSpanCategory.CompileTime );
                }

                // Mark the node.
                var colorFromAnnotation = node.GetColorFromAnnotation();
                if ( colorFromAnnotation != TextSpanCategory.Default )
                {
                    this.Mark( node, colorFromAnnotation );
                }

            }

            base.DefaultVisit( node );
        }


        private void Mark( SyntaxNode? node, TextSpanCategory category, bool includeTrivias = false )
        {
            if ( node != null )
            {
                this.Mark(  node.Span, category );

                if ( includeTrivias )
                {
                    this.Mark( node.GetLeadingTrivia(), category );
                    this.Mark( node.GetTrailingTrivia(), category );
                }
            }
        }

        private void Mark( SyntaxTokenList list, TextSpanCategory category, bool includeTrivias = false )
        {
            foreach ( var item in list)
            {
                this.Mark( item, category, includeTrivias );
            }
        }

        private void Mark<T>( SyntaxList<T> list, TextSpanCategory category, bool includeTrivias = false ) where T : SyntaxNode
        {
            foreach ( var item in list )
            {
                this.Mark( item, category, includeTrivias );
            }
        }


        private void Mark( SyntaxToken token, TextSpanCategory category, bool includeTrivias = false )
        {
                this.Mark( token.Span, category );

            if ( includeTrivias )
            {
                this.Mark( token.LeadingTrivia, category );
                this.Mark( token.TrailingTrivia, category );
            }
        }

        private void Mark( SyntaxTriviaList triviaList, TextSpanCategory category )
        {
            foreach ( var trivia in triviaList )
            {
                char previousChar = trivia.Span.Start == 0 ? '\0' : this._sourceString[trivia.Span.Start - 1];
                int triviaStart = trivia.Span.Start;


                // If we have an identing trivia, trim the start of the span.
                if ( previousChar == '\n' || previousChar == '\r' )
                {
                    // Trim the trivia if it starts with an end line.
                    for ( ; triviaStart < trivia.Span.End && char.IsWhiteSpace( this._sourceString[triviaStart] ); triviaStart++ ) { }
                }

                if ( triviaStart != trivia.Span.End )
                {
                    this._textSpans.Mark( TextSpan.FromBounds( triviaStart, trivia.Span.End ), category );
                }
            }
        }

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
                this.Mark( node.Identifier, TextSpanCategory.CompileTimeVariable );
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