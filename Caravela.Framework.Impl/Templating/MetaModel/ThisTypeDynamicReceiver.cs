// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// An implementation of <see cref="IDynamicExpression"/> that represents a <see cref="INamedType"/> and allows to access
    /// its static members dynamically.
    /// </summary>
    internal class ThisTypeDynamicReceiver : IDynamicReceiver
    {
        private readonly INamedType _type;
        private readonly AspectReferenceSpecification _linkerAnnotation;

        public ThisTypeDynamicReceiver( INamedType type, AspectReferenceSpecification linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null ) => throw new NotSupportedException();

        public bool IsAssignable => false;

        public IType Type => this._type;

        public RuntimeExpression CreateMemberAccessExpression( string member )
            => new(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this._type.GetSymbol() ),
                        SyntaxFactory.IdentifierName( SyntaxFactory.Identifier( member ) ) )
                    .WithAspectReferenceAnnotation( this._linkerAnnotation ),
                this._type.Compilation );

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}