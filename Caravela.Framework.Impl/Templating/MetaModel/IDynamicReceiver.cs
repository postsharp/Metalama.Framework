// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// An <see cref="IDynamicExpression"/> that can be always be used in a <see cref="MemberAccessExpressionSyntax"/>,
    /// but not necessarily as a value itself. This is used to represent <see cref="meta.This"/>. The value represents the current value
    /// or the current type and can be used to allow access to instance or static members.
    /// </summary>
    internal interface IDynamicReceiver : IDynamicExpression
    {
        RuntimeExpression CreateMemberAccessExpression( string member );
    }
}