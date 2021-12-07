// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Represents a statement, which can be inserted into run-time code using the <see cref="meta.InsertStatement(Metalama.Framework.Code.SyntaxBuilders.IStatement)"/>
    /// method.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IStatement { }
}