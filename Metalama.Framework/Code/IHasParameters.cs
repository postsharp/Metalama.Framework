// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a method, constructor, or indexer.
    /// </summary>
    public interface IHasParameters : IMember
    {
        /// <summary>
        /// Gets the list of parameters of the current method (but not the return parameter).
        /// </summary>
        IParameterList Parameters { get; }
    }
}