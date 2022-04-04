﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Interface to be implemented by any compile-time object that can be serialized into a run-time expression.
    /// </summary>
    [RunTimeOrCompileTime]
    public interface IExpressionBuilder
    {
        /// <summary>
        /// Converts the current object into a run-time expression. 
        /// </summary>
        IExpression ToExpression();
    }
}