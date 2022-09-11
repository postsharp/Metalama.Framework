// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Exposes a <see cref="Type"/> property.
    /// </summary>
    [CompileTime]
    public interface IHasType
    {
        /// <summary>
        /// Gets the type of the expression, member, or parameter.
        /// </summary>
        IType Type { get; }
    }
}