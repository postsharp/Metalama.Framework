// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class AnnotatingSyntaxGenerator
    {
        private readonly SyntaxGenerator _syntaxGenerator;

        public AnnotatingSyntaxGenerator( SyntaxGenerator syntaxGenerator )
        {
            this._syntaxGenerator = syntaxGenerator;
        }

        public TypeOfExpressionSyntax TypeOfExpression( ITypeSymbol type )
        {
            var typeSyntax = this.TypeExpression( type.WithNullableAnnotation( NullableAnnotation.NotAnnotated ) );

            var rewriter = type switch
            {
                INamedTypeSymbol { IsGenericType: true } genericType when genericType.IsGenericTypeDefinition() => GenericDefinitionTypeOfRewriter.Instance,
                INamedTypeSymbol { IsGenericType: true } => new GenericInstanceTypeOfRewriter( type ),
                _ => TypeOfRewriter.Instance
            };

            var rewrittenTypeSyntax = rewriter.Visit( typeSyntax );

            return (TypeOfExpressionSyntax) this._syntaxGenerator.TypeOfExpression( rewrittenTypeSyntax );
        }

        public TypeSyntax TypeExpression( ITypeSymbol symbol )
            => (TypeSyntax) this._syntaxGenerator.TypeExpression( symbol )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public SimpleNameSyntax GenericName( string methodName, IEnumerable<ITypeSymbol> @select )
            => (SimpleNameSyntax) this._syntaxGenerator.GenericName( methodName, select )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public ExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
            => (ExpressionSyntax) this._syntaxGenerator.DefaultExpression( typeSymbol )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax type, IEnumerable<SyntaxNode> elements )
        {
            var array = (ArrayCreationExpressionSyntax) this._syntaxGenerator.ArrayCreationExpression( type, elements );

            return array.WithType( array.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
        }

        public TypeSyntax TypeExpression( SpecialType specialType )
            => (TypeSyntax) this._syntaxGenerator.TypeExpression( specialType )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public CastExpressionSyntax CastExpression( ITypeSymbol targetTypeSymbol, ExpressionSyntax expression )
        {
            var cast = (CastExpressionSyntax) this._syntaxGenerator.CastExpression( targetTypeSymbol, expression );

            return cast.WithType( cast.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
        }

        public ExpressionSyntax NameExpression( INamespaceOrTypeSymbol symbol )
        {
            ExpressionSyntax expression;

            switch ( symbol )
            {
                case ITypeSymbol typeSymbol:
                    if ( typeSymbol.NullableAnnotation == NullableAnnotation.Annotated )
                    {
                        return NullableType(
                            (TypeSyntax) this._syntaxGenerator.NameExpression( typeSymbol.WithNullableAnnotation( NullableAnnotation.None ) ) );
                    }
                    else
                    {
                        expression = (ExpressionSyntax) this._syntaxGenerator.NameExpression( typeSymbol );
                    }

                    break;

                case INamespaceSymbol namespaceSymbol:
                    expression = (ExpressionSyntax) this._syntaxGenerator.NameExpression( namespaceSymbol );

                    break;

                default:
                    throw new AssertionFailedException();
            }

            return expression.WithAdditionalAnnotations( Simplifier.Annotation );
        }

        public ThisExpressionSyntax ThisExpression() => (ThisExpressionSyntax) this._syntaxGenerator.ThisExpression();

        public LiteralExpressionSyntax LiteralExpression( object literal ) => (LiteralExpressionSyntax) this._syntaxGenerator.LiteralExpression( literal );

        public IdentifierNameSyntax IdentifierName( string identifier ) => (IdentifierNameSyntax) this._syntaxGenerator.IdentifierName( identifier );

        public TypeSyntax ArrayTypeExpression( ExpressionSyntax type )
        {
            var arrayType = (ArrayTypeSyntax) this._syntaxGenerator.ArrayTypeExpression( type ).WithAdditionalAnnotations( Simplifier.Annotation );

            // Roslyn does not specify the rank properly so it needs to be fixed up.

            return arrayType.WithRankSpecifiers(
                SingletonList( ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) );
        }
    }
}