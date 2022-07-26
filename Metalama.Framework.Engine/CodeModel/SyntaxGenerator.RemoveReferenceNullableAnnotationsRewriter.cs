// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    internal partial class OurSyntaxGenerator
    {
        private class RemoveReferenceNullableAnnotationsRewriter : SafeSyntaxRewriter
        {
            private ITypeSymbol _type;

            public RemoveReferenceNullableAnnotationsRewriter( ITypeSymbol type )
            {
                this._type = type;
            }

            public override SyntaxNode? VisitGenericName( GenericNameSyntax node )
            {
                var type = (INamedTypeSymbol) this._type;

                var argumentsCount = node.TypeArgumentList.Arguments.Count;
                var typeArguments = new TypeSyntax[argumentsCount];

                for ( var i = 0; i < argumentsCount; i++ )
                {
                    using ( this.WithType( type.TypeArguments[i] ) )
                    {
                        typeArguments[i] = (TypeSyntax) this.Visit( node.TypeArgumentList.Arguments[i] );
                    }
                }

                return node.WithTypeArgumentList( SyntaxFactory.TypeArgumentList( SyntaxFactory.SeparatedList( typeArguments ) ) );
            }

            public override SyntaxNode? VisitArrayType( ArrayTypeSyntax node )
            {
                var type = (IArrayTypeSymbol) this._type;

                using ( this.WithType( type.ElementType ) )
                {
                    return base.VisitArrayType( node );
                }
            }

            public override SyntaxNode? VisitTupleType( TupleTypeSyntax node )
            {
                var type = (INamedTypeSymbol) this._type;

                if ( type.NullableAnnotation == NullableAnnotation.Annotated )
                {
                    type = (INamedTypeSymbol) type.TypeArguments[0];
                }

                var elements = new TupleElementSyntax[node.Elements.Count];

                for ( var i = 0; i < node.Elements.Count; i++ )
                {
                    using ( this.WithType( type.TupleElements[i].Type ) )
                    {
                        elements[i] = (TupleElementSyntax) this.VisitTupleElement( node.Elements[i] )!;
                    }
                }

                return node.WithElements( SyntaxFactory.SeparatedList( elements ) );
            }

            public override SyntaxNode? VisitFunctionPointerType( FunctionPointerTypeSyntax node )
            {
                var type = (IFunctionPointerTypeSymbol) this._type;

                var parameters = new FunctionPointerParameterSyntax[node.ParameterList.Parameters.Count];

                var lastParameterIndex = node.ParameterList.Parameters.Count - 1;

                for ( var i = 0; i < node.ParameterList.Parameters.Count; i++ )
                {
                    var parameterType = i == lastParameterIndex ? type.Signature.ReturnType : type.Signature.Parameters[i].Type;

                    using ( this.WithType( parameterType ) )
                    {
                        parameters[i] = (FunctionPointerParameterSyntax) this.VisitFunctionPointerParameter( node.ParameterList.Parameters[i] )!;
                    }
                }

                return node.WithParameterList( SyntaxFactory.FunctionPointerParameterList( SyntaxFactory.SeparatedList( parameters ) ) );
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

            private WithTypeCookie WithType( ITypeSymbol type )
            {
                var cookie = new WithTypeCookie( this, this._type );
                this._type = type;

                return cookie;
            }

            private struct WithTypeCookie : IDisposable
            {
                private readonly RemoveReferenceNullableAnnotationsRewriter _parent;
                private readonly ITypeSymbol _oldType;

                public WithTypeCookie( RemoveReferenceNullableAnnotationsRewriter parent, ITypeSymbol oldType )
                {
                    this._parent = parent;
                    this._oldType = oldType;
                }

                public void Dispose()
                {
                    this._parent._type = this._oldType;
                }
            }
        }
    }
}