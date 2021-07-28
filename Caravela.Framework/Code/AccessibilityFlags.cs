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