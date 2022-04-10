// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// A compile-time representation of a run-time expression.
    /// </summary>
    [CompileTime]
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