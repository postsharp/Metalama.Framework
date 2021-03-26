// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal abstract partial class MetaSyntaxRewriter
    {
        protected partial class MetaSyntaxFactoryImpl
        {

            private readonly ReflectionMapper _reflectionMapper;

            public MetaSyntaxFactoryImpl( Compilation compilation )
            {
                this._reflectionMapper = new ReflectionMapper( compilation );
            }

            public TypeSyntax Type( Type type ) => this._reflectionMapper.GetTypeNameSyntax( type );

            public TypeSyntax Type( ITypeSymbol type ) => (TypeSyntax) CSharpSyntaxGenerator.Instance.NameExpression( type );
            
            public ExpressionSyntax NamespaceOrType( INamespaceOrTypeSymbol type ) => (ExpressionSyntax) CSharpSyntaxGenerator.Instance.NameExpression( type );

            public TypeSyntax GenericType( Type type, params TypeSyntax[] genericParameters )
            {
                var qualifiedName = (QualifiedNameSyntax) this.Type( type );
                return SyntaxFactory.QualifiedName(
                    qualifiedName.Left,
                    ((GenericNameSyntax) qualifiedName.Right).WithTypeArgumentList(
                        SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( genericParameters ) ) ) );
            }

            public MemberAccessExpressionSyntax SyntaxFactoryMethod( string name )
                => SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this.Type( typeof( SyntaxFactory ) ), SyntaxFactory.IdentifierName( name ) );

            public MemberAccessExpressionSyntax GenericSyntaxFactoryMethod( string name, params TypeSyntax[] typeArguments )
                => SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this.Type( typeof( SyntaxFactory ) ),
                    SyntaxFactory.GenericName( SyntaxFactory.Identifier( name ),
                        SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList<TypeSyntax>( typeArguments ) ) ) );

            public LiteralExpressionSyntax LiteralExpression( string s ) => SyntaxFactory.LiteralExpression( SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal( s ) );

            public ArrayTypeSyntax ArrayType<T>()
            {
                return SyntaxFactory.ArrayType( this.Type( typeof( T ) ) ).WithRankSpecifiers(
                    SyntaxFactory.SingletonList( SyntaxFactory.ArrayRankSpecifier( SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>( SyntaxFactory.OmittedArraySizeExpression() ) ) ) );
            }

            public ExpressionSyntax Kind( SyntaxKind kind ) =>
                SyntaxFactory.MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this.Type( typeof( SyntaxKind ) ), SyntaxFactory.IdentifierName( kind.ToString() ) );
        }
    }
}