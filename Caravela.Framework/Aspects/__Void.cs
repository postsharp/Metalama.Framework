// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.ComponentModel;

// ReSharper disable InconsistentNaming

namespace Caravela.Framework.Aspects
{
    
    // We cannot mark this type as obsolete because it ends up being reported in user code.
    
    /// <summary>
    /// A fake type that replaces <c>void</c> in template-generated code. Never use in user code.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
#pragma warning disable IDE1006, SA1300 // Naming Styles
    public readonly struct __Void
#pragma warning restore IDE1006, SA1300 // Naming Styles
    {
        public override string ToString() => "void";
    }
}