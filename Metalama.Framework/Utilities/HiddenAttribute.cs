// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Utilities
{
    /// <summary>
    /// Specifies how the interface must be hidden by tools like our LinqPad adapter.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface )]
    public sealed class HiddenAttribute : Attribute { }
}