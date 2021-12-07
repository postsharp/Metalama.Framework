// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// A compile-time representation of a run-time expression.
    /// </summary>
    [CompileTimeOnly]
    [InternalImplement]
    public interface IExpression : IHasType
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="Value"/> can be set.
        /// </summary>
        bool IsAssignable { get; }

        /// <summary>
        /// Gets or sets the expression value.
        /// </summary>
        dynamic? Value { get; set; }
    }
}