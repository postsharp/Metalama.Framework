// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// An implementation of <see cref="IDynamicExpression"/> that represents <c>this</c> and allows to access its instance members dynamically.
    /// </summary>
    internal class ThisInstanceDynamicReceiver : IDynamicReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisInstanceDynamicReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public RuntimeExpression CreateExpression( string? expressionText, Location? location = null ) => new( ThisExpression(), this._type );

        public bool IsAssignable => this._type.TypeKind == TypeKind.Struct;

        public IType Type => this._type;

        RuntimeExpression IDynamicReceiver.CreateMemberAccessExpression( string member )
            => new(
                MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                this._type );

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }

        // TODO: Add linker annotations.
    }
}