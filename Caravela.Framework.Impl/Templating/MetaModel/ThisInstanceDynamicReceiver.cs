// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// An implementation of <see cref="IDynamicExpression"/> that represents <c>this</c> and allows to access its instance members dynamically.
    /// </summary>
    internal class ThisInstanceDynamicReceiver : IDynamicReceiver
    {
        private readonly INamedType _type;
        private readonly LinkerAnnotation _linkerAnnotation;

        public ThisInstanceDynamicReceiver( INamedType type, LinkerAnnotation linkerAnnotation )
        {
            this._type = type;
            this._linkerAnnotation = linkerAnnotation;
        }

        public RuntimeExpression? CreateExpression( string? expressionText, Location? location = null ) => new( ThisExpression(), this._type );

        RuntimeExpression IDynamicReceiver.CreateMemberAccessExpression( string member )
            => new( MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( Identifier( member ) ) )
                        .AddLinkerAnnotation( this._linkerAnnotation ) );

        // TODO: Add linker annotations.
    }
}