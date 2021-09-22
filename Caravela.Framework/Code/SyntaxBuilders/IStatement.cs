// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Code.SyntaxBuilders
{
    /// <summary>
    /// Represents a statement, which can be inserted into run-time code using the <see cref="meta.InsertStatement(Caravela.Framework.Code.SyntaxBuilders.IStatement)"/>
    /// method.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IStatement { }
}