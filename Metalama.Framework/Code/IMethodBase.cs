// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Reflection;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a method or a constructor.
    /// </summary>
    public interface IMethodBase : IHasParameters
    {
        /// <summary>
        /// Gets a <see cref="MethodBase"/> that represents the current method or constructor at run time.
        /// </summary>
        /// <returns>A <see cref="MethodBase"/> that can be used only in run-time code.</returns>
        MethodBase ToMethodBase();
    }
}