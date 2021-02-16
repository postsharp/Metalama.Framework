using System;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Templating
{
    // TODO: This class is essentially a UI concern so it should be moved into a different assembly e.g. DesignTime.

    /// <summary>
    /// Produces a <see cref="ClassifiedTextSpanCollection"/> with compile-time code given
    /// a syntax tree annotated with <see cref="TemplateAnnotator"/>.
    /// </summary>
    public sealed partial class TextSpanClassifier : CSharpSyntaxWalker
    {
        private readonly ClassifiedTextSpanCollection _classifiedTextSpans = new ClassifiedTextSpanCollection();
        private readonly SourceText _sourceText;
        private readonly bool _visitUnmarkedTypes;
        private readonly string _sourceString;
        private readonly MarkAllChildrenWalker _markAllChildrenWalker;
        private bool _isInTemplate;

#pragma warning disable 618

        // ReSharper disable once IntroduceOptionalParameters.Global
        public TextSpanClassifier( SourceText sourceText ) : this( sourceText, false )
#pragma warning restore 618
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextSpanClassifier"/> class and specifies a backward-compatibility flag.
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="visitUnmarkedTypes">This is for backward compatibility with AspectWorkbench because
        /// test aspect classes are not marked at compile time.</param>
        [Obsolete( "Mark test classes as compile time and call the other constructor." )]
        public TextSpanClassifier( SourceText sourceText, bool visitUnmarkedTypes )
        {
            this._sourceText = sourceText;
            this._visitUnmarkedTypes = visitUnmarkedTypes;
            this._sourceString = sourceText.ToString();
            this._markAllChildrenWalker = new MarkAllChildrenWalker( this );
        }

        public IReadOnlyClassifiedTextSpanCollection ClassifiedTextSpans => this._classifiedTextSpans;

        private static bool ShouldMarkTrivias( TextSpanClassification classification ) =>
            classification switch
            {
                TextSpanClassification.CompileTime => true,
                TextSpanClassification.RunTime => true,
                _ => false
            };

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            if ( node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( node.Modifiers, TextSpanClassification.CompileTime );
                this.Mark( node.Keyword, TextSpanClassification.CompileTime );
                this.Mark( node.OpenBraceToken, TextSpanClassification.CompileTime );
                this.Mark( node.CloseBraceToken, TextSpanClassification.CompileTime );
                this.Mark( node.ConstraintClauses, TextSpanClassification.CompileTime );
                this.Mark( node.Identifier, TextSpanClassification.CompileTime );
                this.Mark( node.BaseList, TextSpanClassification.CompileTime );
                base.VisitClassDeclaration( node );
            }
            else if ( this._visitUnmarkedTypes )
            {
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

                this.Mark( node.ReturnType, TextSpanClassification.CompileTime );
                this.Mark( node.Identifier, TextSpanClassification.CompileTime );
                this.Mark( node.ParameterList, TextSpanClassification.CompileTime );
                this.Mark( node.Modifiers, TextSpanClassification.CompileTime );
                if ( node.Body != null )
                {
                    this.Mark( node.Body.OpenBraceToken, TextSpanClassification.CompileTime );
                    this.Mark( node.Body.CloseBraceToken, TextSpanClassification.CompileTime );
                }

                // The code is run-time by default in a template method.
                this.Mark( node.Body, TextSpanClassification.RunTime );
                this.Mark( node.ExpressionBody, TextSpanClassification.RunTime );

                base.VisitMethodDeclaration( node );

                this._isInTemplate = false;
            }
            else if ( !this._visitUnmarkedTypes )
            {
                this.Mark( node, TextSpanClassification.CompileTime );
            }
        }

        private void VisitMember( SyntaxNode node )
        {
            this.Mark( node, TextSpanClassification.CompileTime );
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
                if ( colorFromAnnotation != TextSpanClassification.Default )
                {
                    this.Mark( token, colorFromAnnotation );
                }
            }
        }

        public override void VisitVariableDeclarator( VariableDeclaratorSyntax node )
        {
            base.VisitVariableDeclarator( node );

            var colorFromAnnotation = node.Identifier.GetColorFromAnnotation();
            if ( colorFromAnnotation != TextSpanClassification.Default )
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
                    this.Mark( node, TextSpanClassification.CompileTime );
                }

                // Mark the node.
                var colorFromAnnotation = node.GetColorFromAnnotation();
                if ( colorFromAnnotation != TextSpanClassification.Default )
                {
                    this.Mark( node, colorFromAnnotation );
                }
            }

            base.DefaultVisit( node );
        }

        private void Mark( SyntaxNode? node, TextSpanClassification classification )
        {
            if ( node != null )
            {
                this._markAllChildrenWalker.MarkAll( node, classification );
            }
        }

        private void Mark( SyntaxTokenList list, TextSpanClassification classification )
        {
            foreach ( var item in list )
            {
                this.Mark( item, classification );
            }
        }

        private void Mark<T>( SyntaxList<T> list, TextSpanClassification classification )
            where T : SyntaxNode
        {
            foreach ( var item in list )
            {
                this.Mark( item, classification );
            }
        }

        private void Mark( SyntaxToken token, TextSpanClassification classification )
        {
            this.Mark( token.Span, classification );

            if ( ShouldMarkTrivias( classification ) )
            {
                this.Mark( token.LeadingTrivia, classification );
                this.Mark( token.TrailingTrivia, classification );
            }
        }

        private void Mark( SyntaxTriviaList triviaList, TextSpanClassification classification )
        {
            foreach ( var trivia in triviaList )
            {
                if ( trivia.Kind() == SyntaxKind.SingleLineCommentTrivia ||
                     trivia.Kind() == SyntaxKind.MultiLineCommentTrivia )
                {
                    // Don't highlight comments.
                    continue;
                }

                var previousChar = trivia.Span.Start == 0 ? '\0' : this._sourceString[trivia.Span.Start - 1];
                var triviaStart = trivia.Span.Start;

                // If we have an indenting trivia, trim the start of the span.
                if ( previousChar == '\n' || previousChar == '\r' )
                {
                    // Trim the trivia if it starts with an end line.
                    for (; triviaStart < trivia.Span.End && char.IsWhiteSpace( this._sourceString[triviaStart] ); triviaStart++ )
                    {
                    }
                }

                if ( triviaStart != trivia.Span.End )
                {
                    this._classifiedTextSpans.Add( TextSpan.FromBounds( triviaStart, trivia.Span.End ), classification );
                }
            }
        }

        private void Mark( TextSpan span, TextSpanClassification classification )
        {
            if ( span.IsEmpty )
            {
                return;
            }

#if DEBUG
            // ReSharper disable once UnusedVariable
            var text = this._sourceText.GetSubText( span ).ToString();
#endif

            this._classifiedTextSpans.Add( span, classification );
        }

        public override void VisitLiteralExpression( LiteralExpressionSyntax node )
        {
            // We don't mark literals that are not a part of larger compile-time expressions because it does not bring anything useful.
        }

        public override void VisitIfStatement( IfStatementSyntax node )
        {
            if ( this._isInTemplate && node.GetScopeFromAnnotation() == SymbolDeclarationScope.CompileTimeOnly )
            {
                this.Mark( TextSpan.FromBounds( node.IfKeyword.SpanStart, node.CloseParenToken.Span.End ), TextSpanClassification.CompileTime );
                this.Visit( node.Condition );
                this.VisitCompileTimeStatementNode( node.Statement );

                if ( node.Else != null )
                {
                    this.Mark( node.Else.ElseKeyword, TextSpanClassification.CompileTime );
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
                this.Mark( TextSpan.FromBounds( node.ForEachKeyword.SpanStart, node.CloseParenToken.Span.End ), TextSpanClassification.CompileTime );
                this.Mark( node.Identifier, TextSpanClassification.CompileTimeVariable );
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
                this.Mark( block.OpenBraceToken, TextSpanClassification.CompileTime );
                this.Mark( block.CloseBraceToken, TextSpanClassification.CompileTime );
            }

            this.Visit( statement );
        }
    }
}