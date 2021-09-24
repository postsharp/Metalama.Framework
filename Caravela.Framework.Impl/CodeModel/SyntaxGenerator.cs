// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class SyntaxGenerator
    {
        private readonly Microsoft.CodeAnalysis.Editing.SyntaxGenerator _syntaxGenerator;
        private readonly bool emitNullabilityAnnotations;

        public SyntaxGenerator( Microsoft.CodeAnalysis.Editing.SyntaxGenerator syntaxGenerator, bool emitNullabilityAnnotations )
        {
            this._syntaxGenerator = syntaxGenerator;
            this.emitNullabilityAnnotations = emitNullabilityAnnotations;
        }

        public TypeOfExpressionSyntax TypeOfExpression( ITypeSymbol type )
        {
            var typeSyntax = this.Type( type.WithNullableAnnotation( NullableAnnotation.NotAnnotated ) );

            if ( type is INamedTypeSymbol { IsGenericType: true } namedType )
            {
                if ( namedType.IsGenericTypeDefinition() )
                {
                    // In generic definitions, we must remove type arguments.
                    typeSyntax = (TypeSyntax) RemoveTypeArgumentsRewriter.Instance.Visit( typeSyntax );
                }
            }

            // In any typeof, we must remove ? annotations of nullable types.
            typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( type ).Visit( typeSyntax );
            
            // In any typeof, we must change dynamic to object.
            typeSyntax = (TypeSyntax) DynamicToVarRewriter.Instance.Visit( typeSyntax );

            
            var rewriter = type switch
            {
                INamedTypeSymbol { IsGenericType: true } genericType when genericType.IsGenericTypeDefinition() => RemoveTypeArgumentsRewriter.Instance,
                INamedTypeSymbol { IsGenericType: true } => new RemoveReferenceNullableAnnotationsRewriter( type ),
                _ => DynamicToVarRewriter.Instance
            };

            var rewrittenTypeSyntax = rewriter.Visit( typeSyntax );

            return (TypeOfExpressionSyntax) this._syntaxGenerator.TypeOfExpression( rewrittenTypeSyntax );
        }

        public TypeSyntax Type( ITypeSymbol symbol )
        {
            var typeSyntax = (TypeSyntax) this._syntaxGenerator.TypeExpression( symbol ).WithAdditionalAnnotations( Simplifier.Annotation );

            if ( !this.emitNullabilityAnnotations )
            {
                typeSyntax = (TypeSyntax) new RemoveReferenceNullableAnnotationsRewriter( symbol ).Visit( typeSyntax );
            }

            return typeSyntax;
        }

        public ExpressionSyntax DefaultExpression( ITypeSymbol typeSymbol )
            =>  SyntaxFactory.DefaultExpression( this.Type( typeSymbol ) )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public ArrayCreationExpressionSyntax ArrayCreationExpression( TypeSyntax type, IEnumerable<SyntaxNode> elements )
        {
            var array = (ArrayCreationExpressionSyntax) this._syntaxGenerator.ArrayCreationExpression( type, elements );

            return array.WithType( array.Type.WithAdditionalAnnotations( Simplifier.Annotation ) );
        }

        public TypeSyntax Type( SpecialType specialType )
            => (TypeSyntax) this._syntaxGenerator.TypeExpression( specialType )
                .WithAdditionalAnnotations( Simplifier.Annotation );

        public CastExpressionSyntax CastExpression( ITypeSymbol targetTypeSymbol, ExpressionSyntax expression )
        {
            switch (expression)
            {
                case BinaryExpressionSyntax:
                case ConditionalExpressionSyntax:
                case CastExpressionSyntax:
                case PrefixUnaryExpressionSyntax:
                    expression = ParenthesizedExpression( expression );

                    break;
            }

            return SyntaxFactory.CastExpression( this.Type( targetTypeSymbol ), expression ).WithAdditionalAnnotations( Simplifier.Annotation );
        }

        public ExpressionSyntax NameExpression( INamespaceOrTypeSymbol symbol )
        {
            ExpressionSyntax expression;

            switch ( symbol )
            {
                case ITypeSymbol typeSymbol:
                    return this.Type( typeSymbol );

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

        public TypeSyntax ArrayTypeExpression( TypeSyntax type )
        {
            var arrayType = (ArrayTypeSyntax) this._syntaxGenerator.ArrayTypeExpression( type ).WithAdditionalAnnotations( Simplifier.Annotation );

            // Roslyn does not specify the rank properly so it needs to be fixed up.

            return arrayType.WithRankSpecifiers(
                SingletonList( ArrayRankSpecifier( SingletonSeparatedList<ExpressionSyntax>( OmittedArraySizeExpression() ) ) ) );
        }

    }
}