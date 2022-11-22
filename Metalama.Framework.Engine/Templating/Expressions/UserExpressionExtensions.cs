// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal static class UserExpressionExtensions
{
    public static ExpressionSyntax ToExpressionSyntax( this IUserExpression userExpression, SyntaxGenerationContext context )
        => userExpression.ToTypedExpressionSyntax( context ).Syntax;
}