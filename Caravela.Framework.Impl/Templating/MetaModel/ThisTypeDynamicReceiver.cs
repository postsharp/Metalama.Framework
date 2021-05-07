// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public ThisTypeDynamicReceiver( INamedType type )
        {
            this._type = type;
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null ) => throw new NotSupportedException();

        public RuntimeExpression CreateMemberAccessExpression( string member )
            => new( SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        (ExpressionSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( this._type.GetSymbol() ),
                        SyntaxFactory.IdentifierName( SyntaxFactory.Identifier( member ) ) ) );
    }
}