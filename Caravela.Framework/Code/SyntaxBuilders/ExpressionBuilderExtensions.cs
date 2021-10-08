// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code.SyntaxBuilders
{
    [CompileTimeOnly]
    public static class ExpressionBuilderExtensions
    {
        public static dynamic? ToValue( this IExpressionBuilder builder ) => builder.ToExpression().Value;

        public static dynamic ToValue( this INotNullExpressionBuilder builder ) => builder.ToExpression().Value!;
    }
}