// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.ComponentModel;
using System.Reflection;

// ReSharper disable All

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable( EditorBrowsableState.Never )]
    [Obfuscation( Exclude = true )]
    internal static class IsExternalInit { }
}