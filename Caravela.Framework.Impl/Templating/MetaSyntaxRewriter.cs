// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// A <see cref="CSharpSyntaxRewriter"/> that transforms a syntax tree
    /// into another syntax tree (typically an expression) that, when executed, returns the original syntax tree.
    /// Parts of the tree may be transformed (typically run-time code), part may be preserved
    /// (typically compile-time code), according to the result of the <see cref="GetTransformationKind"/>
    /// method.
    /// </summary>
    /// <remarks>
    /// Most of this class is machine-generated. This class is meant to be inherited. See the only
    /// inheritor: <see cref="TemplateCompilerRewriter"/>.
    /// </remarks>
    internal abstract partial class MetaSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly Stack<string> _indentTriviaStack = new Stack<string>();
        private readonly IndentRewriter _indentRewriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaSyntaxRewriter"/> class.
        /// </summary>
        /// <param name="compileTimeCompilation">The <see cref="Compilation"/> used to create the compile-time assembly,
        /// possibly with no source code, but with metadata references. Used to resolve symbols in the compile-time assembly.</param>
        protected MetaSyntaxRewriter( Compilation compileTimeCompilation )
        {
            this._indentTriviaStack.Push( "" );
            this._indentRewriter = new IndentRewriter( this );
            this.MetaSyntaxFactory = new MetaSyntaxFactoryImpl( compileTimeCompilation );
        }

        protected MetaSyntaxFactoryImpl MetaSyntaxFactory { get; }

        /// <summary>
        /// Determines how a given <see cref="SyntaxNode"/> must be transformed.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected abstract TransformationKind GetTransformationKind( SyntaxNode node );

        protected void Indent( int level = 1 )
        {
            for ( var i = 0; i < level; i++ )
            {
                // TODO: optimize to avoid string allocation.
                this._indentTriviaStack.Push( this._indentTriviaStack.Peek() + "    " );
            }
        }

        protected void Unindent( int level = 1 )
        {
            for ( var i = 0; i < level; i++ )
            {
                this._indentTriviaStack.Pop();
            }
        }

        protected string GetIndentationWhitespace() => this._indentTriviaStack.Peek();
        
        protected SyntaxTrivia[] GetIndentation( bool lineFeed = true ) =>
            lineFeed
                ? new[] { ElasticCarriageReturnLineFeed, Whitespace( this._indentTriviaStack.Peek() ) }
                : new[] { Whitespace( this.GetIndentationWhitespace() ) };

        protected SyntaxTrivia[] GetLineBreak() => Array.Empty<SyntaxTrivia>();

        /// <summary>
        /// Adds indentation to a <see cref="SyntaxNode"/> and all its children.
        /// </summary>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T DeepIndent<T>( T node )
            where T : SyntaxNode
        {
            node = (T) this._indentRewriter.Visit( node )!;
            return node;
        }

        protected virtual ExpressionSyntax TransformExpression( ExpressionSyntax expression ) => expression;

        protected ExpressionSyntax Transform<T>( T? node )
            where T : SyntaxNode?
        {
            if ( node == null )
            {
                return LiteralExpression( SyntaxKind.NullLiteralExpression );
            }
            else
            {
                var transformedNode = this.Visit( node );

                if ( this.GetTransformationKind( node! ) != TransformationKind.Transform )
                {
                    // The previous call to Visit did not transform node (i.e. transformedNode == node) because the transformation
                    // kind was not set to Transform. The next code tries to "fix" it by using some tricks, but this is not clean.
                    
                    switch ( transformedNode )
                    {
                        case ExpressionSyntax expression:
                            return this.TransformExpression( expression );

                        case ArgumentSyntax argument:
                            return this.TransformArgument( argument );

                        default:
                            throw new AssertionFailedException();
                    }
                }
                else
                {
                    return (ExpressionSyntax) transformedNode;
                }
            }
        }

        protected ExpressionSyntax Transform( SyntaxKind kind ) =>
            this.MetaSyntaxFactory.Kind( kind );

        protected ExpressionSyntax Transform<T>( SeparatedSyntaxList<T> list )
            where T : SyntaxNode
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression( this.MetaSyntaxFactory.GenericType( typeof( SeparatedSyntaxList<> ), this.MetaSyntaxFactory.Type( typeof( T ) ) ) );
            }
            else if ( list.Count == 1 )
            {
                return this.MetaSyntaxFactory.SingletonSeparatedList<T>( this.Transform( list[0] ) );
            }
            else
            {
                return this.MetaSyntaxFactory.SeparatedList2<T>( list.Select( this.Transform ) );
            }
        }

        protected ExpressionSyntax Transform( BracketedArgumentListSyntax? list )
        {
            if ( list == null )
            {
                return DefaultExpression( this.MetaSyntaxFactory.Type( typeof( BracketedArgumentListSyntax ) ) );
            }

            return this.MetaSyntaxFactory.BracketedArgumentList( this.Transform( list.Arguments ) );
        }

        protected ExpressionSyntax Transform( ArgumentListSyntax list ) => this.MetaSyntaxFactory.ArgumentList( this.Transform( list.Arguments ) );

        protected ExpressionSyntax Transform( ParameterListSyntax list ) => this.MetaSyntaxFactory.ParameterList( this.Transform( list.Parameters ) );

        protected ExpressionSyntax Transform( SyntaxTokenList list )
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression( this.MetaSyntaxFactory.Type( typeof( SyntaxTokenList ) ) );
            }

            // TODO: Using default.AddRange is not the right pattern.

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        DefaultExpression(
                            this.MetaSyntaxFactory.Type( typeof( SyntaxTokenList ) ) ),
                        IdentifierName( "AddRange" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                ArrayCreationExpression(
                                    this.MetaSyntaxFactory.ArrayType<SyntaxToken>(),
                                    InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SeparatedList(
                                            list.Select( this.Transform ) ) ) ) ) ) ) );
        }

        protected ExpressionSyntax Transform<T>( SyntaxList<T> list )
            where T : SyntaxNode
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression( this.MetaSyntaxFactory.Type( typeof( SyntaxList<T> ) ) );
            }

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        DefaultExpression( this.MetaSyntaxFactory.Type( typeof( SyntaxList<T> ) ) ),
                        IdentifierName( "AddRange" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                ArrayCreationExpression(
                                    this.MetaSyntaxFactory.ArrayType<T>(),
                                    InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SeparatedList(
                                            list.Select( this.Transform ) ) ) ) ) ) ) );
        }

        protected virtual ExpressionSyntax Transform( SyntaxToken token )
        {
            if ( token.Kind() == SyntaxKind.None )
            {
                return DefaultExpression( this.MetaSyntaxFactory.Type( typeof( SyntaxToken ) ) );
            }
            else if ( token.Kind() == SyntaxKind.IdentifierToken )
            {
                return this.MetaSyntaxFactory.Identifier( this.MetaSyntaxFactory.LiteralExpression( token.Text ) );
            }

            var defaultToken = Token( token.Kind() );

            if ( defaultToken.Value == token.Value )
            {
                // No argument needed.
                return this.MetaSyntaxFactory.Token( this.Transform( token.Kind() ) );
            }
            else
            {
                // Argument needed.

                /*
                 * public static Microsoft.CodeAnalysis.SyntaxToken Token (
                 * Microsoft.CodeAnalysis.SyntaxTriviaList leading,
                 * Microsoft.CodeAnalysis.CSharp.SyntaxKind kind,
                 * string text,
                 * string valueText,
                 * Microsoft.CodeAnalysis.SyntaxTriviaList trailing);
                 */

                return this.MetaSyntaxFactory.Token(
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ),
                    this.Transform( token.Kind() ),
                    this.MetaSyntaxFactory.LiteralExpression( token.Text ),
                    this.MetaSyntaxFactory.LiteralExpression( token.ValueText ),
                    LiteralExpression( SyntaxKind.DefaultLiteralExpression, Token( SyntaxKind.DefaultKeyword ) ));
            }
        }

        protected ExpressionSyntax Transform( bool value )
        {
            return value
                ? LiteralExpression( SyntaxKind.TrueLiteralExpression )
                : LiteralExpression( SyntaxKind.FalseLiteralExpression );
        }

#if DEBUG
        public override TNode? VisitListElement<TNode>( TNode? node )
            where TNode : class
        {
            // This method does not change the behavior of the base method, but it allows for easier debugging
            // of InvalidCastException.
            var transformedNode = this.Visit( node );
            return (TNode?) transformedNode;
        }
#endif

    }
}