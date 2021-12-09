// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Exposes a <see cref="Type"/> property.
    /// </summary>
    [CompileTimeOnly]
    public interface IHasType
    {
        /// <summary>
        /// Gets the type of the expression, member, or parameter.
        /// </summary>
        IType Type { get; }
    }
}