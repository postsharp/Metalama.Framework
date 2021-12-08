// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Impl.Utilities
{
    /// <summary>
    /// Specifies how the interface must be displayed by tools like our LinqPad adapter.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public class DumpBehaviorAttribute : Attribute
    {
        public bool IsHidden { get; set; }
    }
}