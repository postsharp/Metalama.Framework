// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal static class ExpressionExtensions
{
    public static ExpressionSyntax ToExpressionSyntax( this IUserExpression userExpression, SyntaxGenerationContext context )
        => userExpression.ToTypedExpressionSyntax( context ).Syntax;

    public static IUserExpression ToUserExpression( this IExpression expression )
        => expression switch
        {
            null => null!,
            IUserExpression userExpression => userExpression,
            TypedConstant typedConstant => (IUserExpression) SyntaxBuilder.CurrentImplementation.TypedConstant( typedConstant ),
            _ => throw new ArgumentException( $"Expression of type '{expression?.GetType().ToString() ?? "null"}' could not be converted to IUserExpression." )
        };

    public static TypedExpressionSyntax ToTypedExpressionSyntax( this IExpression expression, ISyntaxGenerationContext syntaxGenerationContext )
        => expression.ToUserExpression().ToTypedExpressionSyntax( syntaxGenerationContext );

    public static ExpressionSyntax ToExpressionSyntax( this IExpression expression, SyntaxGenerationContext context )
        => expression.ToTypedExpressionSyntax( context ).Syntax;
}