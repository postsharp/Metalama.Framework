// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Caravela.Framework.Code.ExpressionBuilders;
using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Caravela.Framework.Code.ExpressionBuilders
{

    [CompileTimeOnly]
    public static class ExpressionBuilderExtensions
    {
        public static dynamic? ToValue( this IExpressionBuilder builder ) => builder.ToExpression().Value;
    }
}