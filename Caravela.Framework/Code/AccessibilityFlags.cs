// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    [Flags]
    internal enum AccessibilityFlags
    {
        None = 0,
        SameType = 1,
        DerivedTypeOfFriendAssembly = 2,
        DerivedTypeOfAnyAssembly = 4,
        AnyTypeOfFriendAssembly = 8,
        AnyType = 16,
        Public = SameType | DerivedTypeOfAnyAssembly | DerivedTypeOfFriendAssembly | AnyTypeOfFriendAssembly | AnyType
    }
}