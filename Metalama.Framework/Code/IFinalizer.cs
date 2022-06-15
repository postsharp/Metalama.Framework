// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a method, but not a constructor.
    /// </summary>
    public interface IFinalizer : IMethodBase
    {
        /// <summary>
        /// Gets the base finalizer that is overridden by this finalizer.
        /// </summary>
        IFinalizer? OverriddenFinalizer { get; }
    }
}