// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.CompileTimeContracts;

public readonly struct TypedExpressionSyntax
{
    internal ITypedExpressionSyntax Implementation { get; }

    internal TypedExpressionSyntax( ITypedExpressionSyntax impl )
    {
        this.Implementation = impl;
    }

    public ExpressionSyntax Syntax => this.Implementation.Syntax;

    public static implicit operator ExpressionSyntax?( TypedExpressionSyntax runtimeExpression ) => runtimeExpression.Syntax;

    public static implicit operator ExpressionStatementSyntax?( TypedExpressionSyntax runtimeExpression ) => runtimeExpression.Implementation.ToStatement();
}