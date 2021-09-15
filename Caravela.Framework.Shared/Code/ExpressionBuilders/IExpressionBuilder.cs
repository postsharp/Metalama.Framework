// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Caravela.Framework.Shared.Code.ExpressionBuilders
{
    [CompileTime]
    public interface IExpressionBuilder
    {
        IExpression ToExpression();
    }
}