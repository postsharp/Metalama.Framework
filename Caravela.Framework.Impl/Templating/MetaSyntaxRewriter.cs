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

        public MetaSyntaxRewriter()
        {
            this._indentTriviaStack.Push( "" );
            this._indentRewriter = new IndentRewriter( this );
        }

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

        protected SyntaxTrivia[] GetIndentation( bool lineFeed = true ) =>
            lineFeed
                ? new[] { ElasticCarriageReturnLineFeed, Whitespace( this._indentTriviaStack.Peek() ) }
                : new[] { Whitespace( this._indentTriviaStack.Peek() ) };

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

        protected LiteralExpressionSyntax CreateLiteralExpression( string s )
        {
            return LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( s ) );
        }

        protected ArrayTypeSyntax CreateArrayType<T>()
        {
            return ArrayType(
                    IdentifierName( typeof( T ).Name ) )
                .WithRankSpecifiers(
                    SingletonList(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(
                                OmittedArraySizeExpression() ) ) ) );
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
                    return this.TransformExpression( (ExpressionSyntax) transformedNode );
                }
                else
                {
                    return (ExpressionSyntax) transformedNode;
                }
            }
        }

        protected ExpressionSyntax Transform( SyntaxKind kind )
        {
            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName( "SyntaxKind" ),
                IdentifierName( kind.ToString() ) );
        }

        protected ExpressionSyntax Transform<T>( SeparatedSyntaxList<T> list )
            where T : SyntaxNode
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression(
                    GenericName(
                            Identifier( "SeparatedSyntaxList" ) )
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName( typeof( T ).Name ) ) ) ) );
            }
            else if ( list.Count == 1 )
            {
                return InvocationExpression(
                        GenericName(
                                Identifier( "SingletonSeparatedList" ) )
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName( typeof( T ).Name ) ) ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument( this.Transform( list[0] )
                                    ) ) ) );
            }
            else
            {

                return InvocationExpression(
                        GenericName(
                                Identifier( "SeparatedList" ) )
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName( typeof( T ).Name ) ) ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                list.Select( i => Argument( this.Transform( i ) ) ) ) ) );
            }
        }

        protected ExpressionSyntax Transform( BracketedArgumentListSyntax? list )
        {
            if ( list == null )
            {
                return DefaultExpression( IdentifierName( "BracketedArgumentListSyntax" ) );
            }

            return InvocationExpression(
                    IdentifierName( nameof( BracketedArgumentList ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument( this.Transform( list.Arguments ) ) ) ) );
        }

        protected ExpressionSyntax Transform( ArgumentListSyntax list )
        {
            if ( list.Arguments.Count == 0 )
            {
                return InvocationExpression(
                    IdentifierName( nameof( ArgumentList ) ) );
            }
            else
            {
                return InvocationExpression(
                        IdentifierName( nameof( ArgumentList ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument( this.Transform( list.Arguments ) ) ) ) );
            }
        }

        protected ExpressionSyntax Transform( ParameterListSyntax list )
        {
            if ( list.Parameters.Count == 0 )
            {
                return InvocationExpression(
                    IdentifierName( nameof( ParameterList ) ) );
            }

            return InvocationExpression(
                    IdentifierName( nameof( ParameterList ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument( this.Transform( list.Parameters ) ) ) ) );
        }

        protected ExpressionSyntax Transform( SyntaxTokenList list )
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression( IdentifierName( "SyntaxTokenList" ) );
            }

            // TODO: Using default.AddRange is not the right pattern.

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        DefaultExpression(
                            IdentifierName( "SyntaxTokenList" )
                        ),
                        IdentifierName( "AddRange" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                ArrayCreationExpression(
                                    this.CreateArrayType<SyntaxToken>(),
                                    InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SeparatedList(
                                            list.Select( this.Transform )
                                        ) ) ) ) ) ) );
        }

        protected ExpressionSyntax Transform<T>( SyntaxList<T> list )
            where T : SyntaxNode
        {
            if ( list.Count == 0 )
            {
                return DefaultExpression(
                    GenericName(
                            Identifier( "SyntaxList" ) )
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName( typeof( T ).Name ) ) ) ) );
            }

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        DefaultExpression(
                            GenericName(
                                    Identifier( "SyntaxList" ) )
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName( typeof( T ).Name ) ) ) ) ),
                        IdentifierName( "AddRange" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                ArrayCreationExpression(
                                    this.CreateArrayType<T>(),
                                    InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SeparatedList(
                                            list.Select( this.Transform )
                                        ) ) ) ) ) ) );
        }

        protected ExpressionSyntax Transform( SyntaxToken token )
        {
            if ( token.Kind() == SyntaxKind.None )
            {
                return DefaultExpression( IdentifierName( "SyntaxToken" ) );
            }
            else if ( token.Kind() == SyntaxKind.IdentifierToken )
            {
                return InvocationExpression(
                        IdentifierName( nameof( Identifier ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[]
                            {
                                Argument(this.CreateLiteralExpression(token.Text))
                            } ) ) );
            }

            var defaultToken = Token( token.Kind() );

            if ( defaultToken.Value == token.Value )
            {
                // No argument needed.
                return InvocationExpression(
                        IdentifierName( nameof( Token ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument( this.Transform( token.Kind() ) ) ) ) );
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

                return InvocationExpression(
                        IdentifierName( nameof( Token ) ) )
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[]
                            {
                                Argument(LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token(SyntaxKind.DefaultKeyword))),
                                Token(SyntaxKind.CommaToken),
                                Argument(this.Transform(token.Kind())),
                                Token(SyntaxKind.CommaToken),
                                Argument(this.CreateLiteralExpression(token.Text)),
                                Token(SyntaxKind.CommaToken),
                                Argument(this.CreateLiteralExpression(token.ValueText)),
                                Token(SyntaxKind.CommaToken),
                                Argument(LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token(SyntaxKind.DefaultKeyword))),
                            } ) ) );
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