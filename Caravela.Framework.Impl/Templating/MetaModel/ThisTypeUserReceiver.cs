// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// An implementation of <see cref="IUserExpression"/> that represents a <see cref="INamedType"/> and allows to access
    /// its static members dynamically.
    /// </summary>
    internal class ThisTypeUserReceiver : IUserReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisTypeUserReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public RuntimeExpression ToRunTimeExpression() => throw new NotSupportedException();

        public bool IsAssignable => false;

        public IType Type => this._type;

        public RuntimeExpression CreateMemberAccessExpression( string member )
            => new(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        TemplateExpansionContext.CurrentSyntaxGenerationContext.SyntaxGenerator.Type( this._type.GetSymbol() ),
                        SyntaxFactory.IdentifierName( SyntaxFactory.Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                this._type.Compilation );

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}