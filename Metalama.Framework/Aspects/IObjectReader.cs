// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Exposes as a dictionary the tags passed to an advise.
    /// </summary>
    [CompileTime]
    public interface IObjectReader : IReadOnlyDictionary<string, object?>
    {
        /// <summary>
        /// Gets the source object (typically an object of an anonymous type).
        /// </summary>
        object? Source { get; }
    }
}