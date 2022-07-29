// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Formatting
{
    /// <summary>
    /// Produces a <see cref="ClassifiedTextSpanCollection"/> with compile-time code given
    /// a syntax tree annotated with <see cref="TemplateAnnotator"/>.
    /// </summary>
    public sealed partial class TextSpanClassifier : SafeSyntaxWalker
    {
#if !DEBUG
#pragma warning disable IDE0052 // Remove unread private members

        // ReSharper disable once NotAccessedField.Local
#endif
        private readonly SourceText _sourceText;
#if !DEBUG
#pragma warning restore IDE0052 // Remove unread private members
#endif
        private readonly string _sourceString;
        private readonly MarkAllChildrenWalker _markAllChildrenWalker;
        private bool _isInTemplate;
        private bool _isInCompileTimeType;
        private bool _isRecursiveCall;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextSpanClassifier"/> class and specifies a backward-compatibility flag.
        /// </summary>
        /// <param name="sourceText"></param>
        public TextSpanClassifier( SourceText sourceText )
            : base( SyntaxWalkerDepth.Token )
        {
            this._sourceText = sourceText;
            this._sourceString = sourceText.ToString();
            this._markAllChildrenWalker = new MarkAllChildrenWalker( this );
            this.ClassifiedTextSpans = new ClassifiedTextSpanCollection( sourceText );
        }

        public ClassifiedTextSpanCollection ClassifiedTextSpans { get; }

        private static bool ShouldMarkTrivia( TextSpanClassification classification )
            => classification switch
            {
                TextSpanClassification.CompileTime => true,
                TextSpanClassification.RunTime => true,
                _ => false
            };

        protected override void VisitCore( SyntaxNode? node )
        {
            if ( node == null )
            {
                // Coverage: ignore.
                return;
            }

            var isRecursiveCall = this._isRecursiveCall;
            this._isRecursiveCall = true;

            try
            {
                base.VisitCore( node );
            }
            finally
            {
                this._isRecursiveCall = isRecursiveCall;
            }
        }

        private bool VisitTypeDeclaration<T>( T node, Action<T> visitBase )
            where T : TypeDeclarationSyntax
        {
            var scope = node.GetScopeFromAnnotation();

            if ( scope is TemplatingScope.CompileTimeOnly or TemplatingScope.RunTimeOrCompileTime )
            {
                var oldIsInCompileTimeType = this._isInCompileTimeType;

                this._isInCompileTimeType = true;

                try
                {
                    this.Mark( node.Modifiers, TextSpanClassification.CompileTime );
                    this.Mark( node.Keyword, TextSpanClassification.CompileTime );
                    this.Mark( node.OpenBraceToken, TextSpanClassification.CompileTime );
                    this.Mark( node.CloseBraceToken, TextSpanClassification.CompileTime );
                    this.Mark( node.ConstraintClauses, TextSpanClassification.CompileTime );
                    this.Mark( node.Identifier, TextSpanClassification.CompileTime );
                    this.Mark( node.BaseList, TextSpanClassification.CompileTime );
                    this.Mark( node.AttributeLists, TextSpanClassification.CompileTime );
                    this.Mark( node.TypeParameterList, TextSpanClassification.CompileTime );

                    visitBase( node );

                    return true;
                }
                finally
                {
                    this._isInCompileTimeType = oldIsInCompileTimeType;
                }
            }

            return false;
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitClassDeclaration( n ) );

        public override void VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );

        public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
        {
            if ( this.VisitTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) ) )
            {
                this.Mark( node.ParameterList, TextSpanClassification.CompileTime );
            }
        }

        private void VisitSimpleTypeDeclaration( SyntaxNode node )
        {
            var scope = node.GetScopeFromAnnotation();

            if ( scope is TemplatingScope.CompileTimeOnly or TemplatingScope.RunTimeOrCompileTime )
            {
                this.Mark( node, TextSpanClassification.CompileTime );
            }
        }

        public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => this.VisitSimpleTypeDeclaration( node );

        public override void VisitEnumDeclaration( EnumDeclarationSyntax node ) => this.VisitSimpleTypeDeclaration( node );

        public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            if ( node.IsTemplateFromAnnotation() )
            {
                this._isInTemplate = true;

                // The code is run-time by default in a template method.
                this.Mark( node.Body, TextSpanClassification.RunTime );
                this.Mark( node.ExpressionBody, TextSpanClassification.RunTime );

                base.VisitMethodDeclaration( node );

                this._isInTemplate = false;
            }
            else
            {
                this.VisitMember( node );
            }
        }

        private void VisitMember( SyntaxNode node )
        {
            if ( this._isInCompileTimeType )
            {
                this.Mark( node, TextSpanClassification.CompileTime );
            }
        }

        public override void VisitFieldDeclaration( FieldDeclarationSyntax node ) => this.VisitMember( node );

        public override void VisitEventDeclaration( EventDeclarationSyntax node ) => this.VisitMember( node );

        public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            if ( node.IsTemplateFromAnnotation() ||
                 (node.AccessorList != null && node.AccessorList.Accessors.Any( a => a.IsTemplateFromAnnotation() )) )
            {
                this._isInTemplate = true;

                // The code is run-time by default in a template method.
                this.Mark( node.ExpressionBody, TextSpanClassification.RunTime );

                base.VisitPropertyDeclaration( node );

                this._isInTemplate = false;
            }
            else
            {
                this.VisitMember( node );
            }
        }

        public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
        {
            if ( this._isInTemplate )
            {
                this.Mark( node.Body, TextSpanClassification.RunTime );

                if ( node.ExpressionBody != null )
                {
                    this.Mark( node.ExpressionBody.Expression, TextSpanClassification.RunTime );
                }
            }

            base.VisitAccessorDeclaration( node );
        }

        public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node ) => this.VisitMember( node );

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

            base.VisitToken( token );
        }

        public override void DefaultVisit( SyntaxNode node )
        {
            if ( this._isInTemplate )
            {
                if ( node.GetScopeFromAnnotation().GetValueOrDefault().GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
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

            if ( ShouldMarkTrivia( classification ) )
            {
                this.Mark( token.LeadingTrivia, classification );
                this.Mark( token.TrailingTrivia, classification );
            }
        }

        private void Mark( SyntaxTriviaList triviaList, TextSpanClassification classification )
        {
            foreach ( var trivia in triviaList )
            {
                if ( trivia.IsKind( SyntaxKind.SingleLineCommentTrivia ) ||
                     trivia.IsKind( SyntaxKind.MultiLineCommentTrivia ) )
                {
                    // Don't highlight comments.
                    continue;
                }

                var previousChar = trivia.Span.Start == 0 ? '\0' : this._sourceString[trivia.Span.Start - 1];
                var triviaStart = trivia.Span.Start;
                var triviaEnd = Math.Min( trivia.Span.End, this._sourceString.Length - 1 );

                if ( triviaStart > triviaEnd )
                {
                    // This should not happen.
                    continue;
                }

                // If we have an indenting trivia, trim the start of the span.
                if ( previousChar == '\n' || previousChar == '\r' )
                {
                    // Trim the trivia if it starts with an end line.
                    for ( /*void*/; triviaStart < triviaEnd && char.IsWhiteSpace( this._sourceString[triviaStart] ); triviaStart++ ) { }
                }

                // If we end with an end-of-line or space, trim it.
                for ( /*void*/; triviaEnd > triviaStart && char.IsWhiteSpace( this._sourceString[triviaEnd] ); triviaEnd-- ) { }

                if ( triviaStart != triviaEnd )
                {
                    this.ClassifiedTextSpans.Add( TextSpan.FromBounds( triviaStart, triviaEnd ), classification );
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

            this.ClassifiedTextSpans.Add( span, classification );
        }

        public override void VisitLiteralExpression( LiteralExpressionSyntax node )
        {
            // We don't mark literals that are not a part of larger compile-time expressions because it does not bring anything useful.
        }

        public override void VisitIfStatement( IfStatementSyntax node )
        {
            if ( this._isInTemplate && node.GetScopeFromAnnotation() == TemplatingScope.CompileTimeOnly )
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
            if ( this._isInTemplate && node.GetScopeFromAnnotation() == TemplatingScope.CompileTimeOnly )
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