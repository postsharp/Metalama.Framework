// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global

namespace Metalama.Framework.Engine.Templating
{
    internal partial class MetaSyntaxRewriter
    {
        protected sealed partial class MetaSyntaxFactoryImpl
        {
            public MetaSyntaxFactoryImpl( Compilation compileTimeCompilation )
            {
                this.ReflectionMapper = CompilationContextFactory.GetInstance( compileTimeCompilation ).ReflectionMapper;
            }

            public ReflectionMapper ReflectionMapper { get; }

            public ExpressionSyntax Null => this.LiteralExpression( this.Kind( SyntaxKind.NullLiteralExpression ) );

            public TypeSyntax Type( Type type ) => OurSyntaxGenerator.CompileTime.Type( this.ReflectionMapper.GetTypeSymbol( type ) );

            public static TypeSyntax Type( ITypeSymbol type )
                => type switch
                {
                    IArrayTypeSymbol arrayType => OurSyntaxGenerator.CompileTime.ArrayTypeExpression(
                        OurSyntaxGenerator.CompileTime.Type( arrayType.ElementType ) ),
                    _ => OurSyntaxGenerator.CompileTime.TypeOrNamespace( type )
                };

            public TypeSyntax GenericType( Type type, params TypeSyntax[] genericParameters )
            {
                var qualifiedName = (QualifiedNameSyntax) this.Type( type );

                return SyntaxFactory.QualifiedName(
                    qualifiedName.Left,
                    ((GenericNameSyntax) qualifiedName.Right).WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( genericParameters ) ) ) );
            }

            public MemberAccessExpressionSyntax SyntaxFactoryMethod( string name )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof(SyntaxFactory) ),
                    SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericSyntaxFactoryMethod( string name, params TypeSyntax[] typeArguments )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof(SyntaxFactory) ),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier( name ),
                        SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( typeArguments ) ) ) );

            public ArrayTypeSyntax ArrayType<T>() => SyntaxFactoryEx.ArrayType( this.Type( typeof(T) ) );

            public ExpressionSyntax Kind( SyntaxKind kind )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof(SyntaxKind) ),
                    SyntaxFactory.IdentifierName( kind.ToString() ) );

            public ExpressionSyntax Literal( ExpressionSyntax expression )
            {
                var result = SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Literal) ),
                    SyntaxFactory.ArgumentList( SyntaxFactory.SeparatedList( new[] { SyntaxFactory.Argument( expression ) } ) ) );

                return result;
            }

            public ExpressionSyntax Literal( SyntaxToken token )
                => SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Literal) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( token.Text ) ),
                                SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( token.Value, ObjectDisplayOptions.IncludeTypeSuffix ) )
                            } ) ) );

            public ExpressionSyntax Literal( object value )
                => SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Literal) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList( new[] { SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( value ) ) } ) ) );

            public ExpressionSyntax SingletonSeparatedList<T>( ExpressionSyntax item )
                where T : SyntaxNode
            {
                var itemType = this.Type( typeof(T) );

                var result = SyntaxFactory.InvocationExpression(
                    this.GenericSyntaxFactoryMethod( nameof(SyntaxFactory.SingletonSeparatedList), itemType ),
                    SyntaxFactory.ArgumentList( SyntaxFactory.SeparatedList( new[] { SyntaxFactory.Argument( item ) } ) ) );

                return result;
            }

            public ExpressionSyntax SeparatedList<T>( IEnumerable<ExpressionSyntax> items )
                where T : SyntaxNode
                => this.List<T>( nameof(SyntaxFactory.SeparatedList), items );

            private ExpressionSyntax List<T>( string methodName, IEnumerable<ExpressionSyntax> items )
                where T : SyntaxNode
            {
                var itemType = this.Type( typeof(T) );

                var argument =
                    SyntaxFactoryEx.ArrayCreationExpression(
                        SyntaxFactoryEx.ArrayType( itemType ),
                        SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( items ) ) );

                var result = SyntaxFactory.InvocationExpression(
                    this.GenericSyntaxFactoryMethod( methodName, itemType ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList( new[] { SyntaxFactory.Argument( argument ) } ) ) );

                return result;
            }

            public ExpressionSyntax Identifier( ExpressionSyntax text, bool trailingSpace )
            {
                // SyntaxFactory.Identifier( default, "text", new( SyntaxFactory.Space ) );
                // SyntaxFactory.Identifier( "text" );

                var arguments = trailingSpace
                    ? new[]
                    {
                        SyntaxFactory.Argument( SyntaxFactoryEx.Default ),
                        SyntaxFactory.Argument( text ),
                        SyntaxFactory.Argument(
                            SyntaxFactory.ImplicitObjectCreationExpression(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList( SyntaxFactory.Argument( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Space) ) ) ) ),
                                default ) )
                    }
                    : new[] { SyntaxFactory.Argument( text ) };

                var result = SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof( SyntaxFactory.Identifier) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList( arguments ) ) );

                return result;
            }

            public ExpressionSyntax Identifier(
                ExpressionSyntax leadingTrivia,
                ExpressionSyntax syntaxKind,
                ExpressionSyntax text,
                ExpressionSyntax valueText,
                ExpressionSyntax trailingTrivia )
            {
                var result = SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Identifier) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                SyntaxFactory.Argument( leadingTrivia ),
                                SyntaxFactory.Argument( syntaxKind ),
                                SyntaxFactory.Argument( text ),
                                SyntaxFactory.Argument( valueText ),
                                SyntaxFactory.Argument( trailingTrivia )
                            } ) ) );

                return result;
            }

            public ExpressionSyntax Token( ExpressionSyntax kind )
            {
                var result = SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Token) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { SyntaxFactory.Argument( kind ) } ) ) );

                return result;
            }

            public ExpressionSyntax Token(
                ExpressionSyntax leading,
                ExpressionSyntax kind,
                ExpressionSyntax text,
                ExpressionSyntax valueText,
                ExpressionSyntax trailing )
            {
                var result = SyntaxFactory.InvocationExpression(
                    this.SyntaxFactoryMethod( nameof(SyntaxFactory.Token) ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                SyntaxFactory.Argument( leading ),
                                SyntaxFactory.Argument( kind ),
                                SyntaxFactory.Argument( text ),
                                SyntaxFactory.Argument( valueText ),
                                SyntaxFactory.Argument( trailing )
                            } ) ) );

                return result;
            }

            public ExpressionSyntax List<T>( IEnumerable<ExpressionSyntax> items )
                where T : SyntaxNode
                => this.List<T>( nameof(SyntaxFactory.List), items );
        }
    }
}