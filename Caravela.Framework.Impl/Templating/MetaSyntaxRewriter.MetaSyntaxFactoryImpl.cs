// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class MetaSyntaxRewriter
    {
        protected partial class MetaSyntaxFactoryImpl
        {
            private readonly ReflectionMapper _reflectionMapper;

            public MetaSyntaxFactoryImpl( IServiceProvider serviceProvider, Compilation compileTimeCompilation )
            {
                this._reflectionMapper = serviceProvider.GetService<ReflectionMapperFactory>().GetInstance( compileTimeCompilation );
            }

            public TypeSyntax Type( Type type ) => OurSyntaxGenerator.CompileTime.Type( this._reflectionMapper.GetTypeSymbol( type ) );

#pragma warning disable CA1822 // Mark members as static
            public TypeSyntax Type( ITypeSymbol type )
                => type switch
                {
                    IArrayTypeSymbol arrayType => OurSyntaxGenerator.CompileTime.ArrayTypeExpression(
                        OurSyntaxGenerator.CompileTime.Type( arrayType.ElementType ) ),
                    _ => (TypeSyntax) OurSyntaxGenerator.CompileTime.NameExpression( type )
                };

            public ExpressionSyntax NamespaceOrType( INamespaceOrTypeSymbol type ) => OurSyntaxGenerator.CompileTime.NameExpression( type );
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
                    this.Type( typeof( SyntaxFactory ) ),
                    SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericSyntaxFactoryMethod( string name, params TypeSyntax[] typeArguments )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof( SyntaxFactory ) ),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier( name ),
                        SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( typeArguments ) ) ) );

            public ExpressionSyntax Literal( string? s ) => s == null ? SyntaxFactoryEx.Null : this.Literal( SyntaxFactoryEx.LiteralExpression( s ) );

            public ExpressionSyntax Literal( char c ) => this.Literal( SyntaxFactoryEx.LiteralExpression( c ) );

            public ExpressionSyntax Literal( int i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( uint i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( long i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( ulong i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( short i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( ushort i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( double i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( float i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ExpressionSyntax Literal( decimal i ) => this.Literal( SyntaxFactoryEx.LiteralExpression( i ) );

            public ArrayTypeSyntax ArrayType<T>()
            {
                return SyntaxFactory.ArrayType( this.Type( typeof( T ) ) )
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) );
            }

            public ExpressionSyntax Kind( SyntaxKind kind )
                => SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this.Type( typeof( SyntaxKind ) ),
                    SyntaxFactory.IdentifierName( kind.ToString() ) );

            public ExpressionSyntax Literal( SyntaxToken literal )
                => literal.Value switch
                {
                    string s => this.Literal( s ),
                    char s => this.Literal( s ),
                    int s => this.Literal( s ),
                    uint s => this.Literal( s ),
                    long s => this.Literal( s ),
                    ulong s => this.Literal( s ),
                    short s => this.Literal( s ),
                    ushort s => this.Literal( s ),
                    double s => this.Literal( s ),
                    float s => this.Literal( s ),
                    decimal s => this.Literal( s ),
                    _ => throw new ArgumentOutOfRangeException()
                };
        }
    }
}