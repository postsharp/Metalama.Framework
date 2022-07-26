// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.CodeModel
{
    [Flags]
    internal enum ModifierCategories
    {
        Accessibility = 1,
        Inheritance = 2,
        Async = 4,
        Static = 8,
        ReadOnly = 16,
        Unsafe = 32,
        Volatile = 64,
        All = Accessibility | Inheritance | Async | Static | ReadOnly | Unsafe | Volatile
    }
}