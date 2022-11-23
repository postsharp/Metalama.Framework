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
        protected partial class MetaSyntaxFactoryImpl
        {
            public MetaSyntaxFactoryImpl( ProjectServiceProvider serviceProvider, Compilation compileTimeCompilation )
            {
                this.ReflectionMapper = serviceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( compileTimeCompilation ).ReflectionMapper;
            }

            public ReflectionMapper ReflectionMapper { get; }

            public ExpressionSyntax Null => this.LiteralExpression( this.Kind( SyntaxKind.NullLiteralExpression ) );

            public ExpressionSyntax Default
                => this.LiteralExpression(
                    this.Kind( SyntaxKind.DefaultLiteralExpression ),
                    this.Token( this.Kind( SyntaxKind.DefaultKeyword ) ) );

            public TypeSyntax Type( Type type ) => OurSyntaxGenerator.CompileTime.Type( this.ReflectionMapper.GetTypeSymbol( type ) );

#pragma warning disable CA1822 // Mark members as static
            public TypeSyntax Type( ITypeSymbol type )
                => type switch
                {
                    IArrayTypeSymbol arrayType => OurSyntaxGenerator.CompileTime.ArrayTypeExpression(
                        OurSyntaxGenerator.CompileTime.Type( arrayType.ElementType ) ),
                    _ => (TypeSyntax) OurSyntaxGenerator.CompileTime.TypeOrNamespace( type )
                };

            public ExpressionSyntax NamespaceOrType( INamespaceOrTypeSymbol type ) => OurSyntaxGenerator.CompileTime.TypeOrNamespace( type );
#pragma warning restore CA1822 // Mark members as static

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

            public ExpressionSyntax LiteralExpression( string? s )
                => s == null ? SyntaxFactoryEx.Null : this.MakeLiteralExpression( SyntaxFactoryEx.LiteralNonNullExpression( s ) );

            public ExpressionSyntax LiteralExpression( char c ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( c ) );

            public ExpressionSyntax LiteralExpression( int i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( uint i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( long i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( ulong i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( short i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( ushort i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( double i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( float i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax LiteralExpression( decimal i ) => this.MakeLiteralExpression( SyntaxFactoryEx.LiteralExpression( i ) );

            private InvocationExpressionSyntax MakeLiteralExpression( LiteralExpressionSyntax i )
            {
                return this.LiteralExpression(
                    this.Kind( i.Kind() ),
                    this.Literal( i.Token ) );
            }

            public ArrayTypeSyntax ArrayType<T>()
            {
                return SyntaxFactory.ArrayType( this.Type( typeof(T) ) )
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) );
            }

            public ExpressionSyntax Kind( SyntaxKind kind )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof(SyntaxKind) ),
                    SyntaxFactory.IdentifierName( kind.ToString() ) );

            public ExpressionSyntax LiteralExpression( SyntaxToken literal )
                => literal.Value switch
                {
                    string s => this.LiteralExpression( s ),
                    char s => this.LiteralExpression( s ),
                    int s => this.LiteralExpression( s ),
                    uint s => this.LiteralExpression( s ),
                    long s => this.LiteralExpression( s ),
                    ulong s => this.LiteralExpression( s ),
                    short s => this.LiteralExpression( s ),
                    ushort s => this.LiteralExpression( s ),
                    double s => this.LiteralExpression( s ),
                    float s => this.LiteralExpression( s ),
                    decimal s => this.LiteralExpression( s ),
                    _ => throw new ArgumentOutOfRangeException()
                };

            public ExpressionSyntax LiteralExpression( object literal )
                => literal switch
                {
                    string s => this.LiteralExpression( s ),
                    char s => this.LiteralExpression( s ),
                    int s => this.LiteralExpression( s ),
                    uint s => this.LiteralExpression( s ),
                    long s => this.LiteralExpression( s ),
                    ulong s => this.LiteralExpression( s ),
                    short s => this.LiteralExpression( s ),
                    ushort s => this.LiteralExpression( s ),
                    double s => this.LiteralExpression( s ),
                    float s => this.LiteralExpression( s ),
                    decimal s => this.LiteralExpression( s ),
                    _ => throw new ArgumentOutOfRangeException()
                };

            public ExpressionSyntax Literal( ExpressionSyntax expression )
            {
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Literal) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { SyntaxFactory.Argument( expression ) } ) ) );

                return result;
            }

            public ExpressionSyntax Literal( SyntaxToken token ) => token.IsKind( SyntaxKind.NullKeyword ) ? this.Null : this.Literal( token.Value! );

            public ExpressionSyntax Literal( object value )
            {
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Literal) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[] { SyntaxFactory.Argument( SyntaxFactoryEx.LiteralExpression( value ) ) } ) ) );

                return result;
            }

            public ExpressionSyntax SingletonSeparatedList<T>( ExpressionSyntax item )
                where T : SyntaxNode
            {
                var itemType = this.Type( typeof(T) );

                var result = SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( nameof(SyntaxFactory.SingletonSeparatedList), itemType ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { SyntaxFactory.Argument( item ) } ) ) );

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
                    SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType( itemType )
                            .WithRankSpecifiers(
                                SyntaxFactory.SingletonList(
                                    SyntaxFactory.ArrayRankSpecifier(
                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) ),
                        SyntaxFactory.InitializerExpression( SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList( items ) ) );

                var result = SyntaxFactory.InvocationExpression( this.GenericSyntaxFactoryMethod( methodName, itemType ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { SyntaxFactory.Argument( argument ) } ) ) );

                return result.NormalizeWhitespace();
            }

            public ExpressionSyntax Identifier( ExpressionSyntax text )
            {
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Identifier) ) )
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList<ArgumentSyntax>( new SyntaxNodeOrToken[] { SyntaxFactory.Argument( text ) } ) ) );

                return result;
            }

            public ExpressionSyntax Identifier(
                ExpressionSyntax leadingTrivia,
                ExpressionSyntax syntaxKind,
                ExpressionSyntax text,
                ExpressionSyntax valueText,
                ExpressionSyntax trailingTrivia )
            {
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Identifier) ) )
                    .WithArgumentList(
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
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Token) ) )
                    .WithArgumentList(
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
                var result = SyntaxFactory.InvocationExpression( this.SyntaxFactoryMethod( nameof(SyntaxFactory.Token) ) )
                    .WithArgumentList(
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