// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        private class RemoveReferenceNullableAnnotationsRewriter : CSharpSyntaxRewriter
        {
            private ITypeSymbol _type;

            public RemoveReferenceNullableAnnotationsRewriter( ITypeSymbol type )
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

        public bool IsNullAware { get; }
    }
}