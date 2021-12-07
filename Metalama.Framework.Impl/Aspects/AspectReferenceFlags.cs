// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Provides flags on aspect reference, guiding the aspect linker.
    /// </summary>
    [Flags]
    internal enum AspectReferenceFlags
    {
        /// <summary>
        /// No flags are active on the aspect reference.
        /// </summary>
        None = 0,

        /// <summary>
        /// The reference is not inlineable.
        /// </summary>
        Inlineable = 1
    }
}