﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.SyntaxBuilders
{
    [CompileTimeOnly]
    public static class ExpressionBuilderExtensions
    {
        /// <summary>
        /// Gets an object that can be used in a run-time expression of a template to represent the result of the current expression builder.
        /// </summary>
        public static dynamic? ToValue( this IExpressionBuilder builder ) => builder.ToExpression().Value;

        /// <summary>
        /// Gets an object that can be used in a run-time expression of a template to represent the result of the current expression builder.
        /// </summary>
        public static dynamic ToValue( this INotNullExpressionBuilder builder ) => builder.ToExpression().Value!;
    }
}