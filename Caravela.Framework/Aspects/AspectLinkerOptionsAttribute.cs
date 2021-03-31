// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Specifies behavior of the aspect linker on the declaration (for testing only).
    /// </summary>
    [AttributeUsage( AttributeTargets.All, AllowMultiple = true )]
    internal sealed class AspectLinkerOptionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the linker inlining of this member is forcefully disabled, even if it would be otherwise possible.
        /// </summary>
        public bool ForceNotInlineable { get; set; }
    }
}
