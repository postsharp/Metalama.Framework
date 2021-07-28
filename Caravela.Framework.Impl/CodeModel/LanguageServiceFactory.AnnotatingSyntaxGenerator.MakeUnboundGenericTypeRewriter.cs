// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static partial class LanguageServiceFactory
    {
        private class UnboundTypeRewriter : CSharpSyntaxRewriter
        {
            public static readonly UnboundTypeRewriter Instance = new();

            private UnboundTypeRewriter() { }

            public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
            {
                // We intentionally don't visit type arguments, because we don't want remove the nested type arguments.

                // Remove the list of type arguments.
                if ( node.TypeArgumentList.Arguments.Count == 1 )
                {
                    return SyntaxFactory.GenericName( node.Identifier );
                }
                else
                {
                    return SyntaxFactory.GenericName( node.Identifier )
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SeparatedList<TypeSyntax>(
                                    node.TypeArgumentList.Arguments.Select( _ => SyntaxFactory.OmittedTypeArgument() ) ) ) );
                }
            }
        }

        private class NullableAnnotationRewriter : CSharpSyntaxRewriter
        {
            private ITypeSymbol _type;

            public NullableAnnotationRewriter( ITypeSymbol type )
            {
                this._type = type;
            }

            public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
            {
                var oldType = (INamedTypeSymbol) this._type;

                var argumentsCount = node.TypeArgumentList.Arguments.Count;
                var typeArguments = new TypeSyntax[argumentsCount];

                for ( var i = 0; i < argumentsCount; i++ )
                {
                    this._type = oldType.TypeArguments[i];
                    typeArguments[i] = (TypeSyntax) this.Visit( node.TypeArgumentList.Arguments[i] );
                }

                this._type = oldType;

                return node.WithTypeArgumentList( SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( typeArguments ) ) );
            }

            public override SyntaxNode? VisitArrayType( ArrayTypeSyntax node )
            {
                var oldType = (IArrayTypeSymbol) this._type;
                this._type = oldType.ElementType;

                try
                {
                    return base.VisitArrayType( node );
                }
                finally
                {
                    this._type = oldType;
                }
            }

            public override SyntaxNode? VisitNullableType( NullableTypeSyntax node )
            {
                if ( this._type.IsReferenceType )
                {
                    // Skip the annotation.
                    return this.Visit( node.ElementType );
                }
                else
                {
                    // Keep it.
                    return base.VisitNullableType( node );
                }
            }
        }
    }
}