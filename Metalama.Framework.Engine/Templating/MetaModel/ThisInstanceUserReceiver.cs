// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis.CSharp;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// An implementation of <see cref="IUserExpression"/> that represents <c>this</c> and allows to access its instance members dynamically.
    /// </summary>
    internal class ThisInstanceUserReceiver : IUserReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisInstanceUserReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public RuntimeExpression ToRunTimeExpression() => new( ThisExpression(), this._type, TemplateExpansionContext.CurrentSyntaxGenerationContext );

        public bool IsAssignable => this._type.TypeKind == TypeKind.Struct;

        public IType Type => this._type;

        RuntimeExpression IUserReceiver.CreateMemberAccessExpression( string member )
            => new(
                MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                this._type,
                TemplateExpansionContext.CurrentSyntaxGenerationContext );

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }

        // TODO: Add linker annotations.
    }
}