using System;

namespace Caravela.Framework.Impl
{
    [Flags]
    internal enum AccessibilityFlags
    {
        None = 0,
        SameType = 1,
        DerivedTypeOfFriendAssembly = 2 | SameType,
        DerivedTypeOfAnyAssembly = 4 | DerivedTypeOfFriendAssembly,
        AnyTypeOfFriendAssembly = 8 | DerivedTypeOfFriendAssembly,
        AnyType = 16 | AnyTypeOfFriendAssembly,

        Private = SameType,
        Public = AnyType,
        Protected = DerivedTypeOfAnyAssembly,
        Internal = AnyTypeOfFriendAssembly,
        PrivateProtected = DerivedTypeOfFriendAssembly,
        ProtectedInternal = DerivedTypeOfAnyAssembly | AnyTypeOfFriendAssembly
    }
}