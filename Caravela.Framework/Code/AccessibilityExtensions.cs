// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Code
{
    internal static class AccessibilityExtensions
    {
        public static bool IsSupersetOf( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == right && left != right;

        public static bool IsSubsetOf( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == left && left != right;

        public static bool IsSupersetOrEqual( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == right;

        public static bool IsSubsetOrEqual( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == left;

        public static AccessibilityFlags ToAccessibilityFlags( this Accessibility accessibility )
            => accessibility switch
            {
                Accessibility.Private => AccessibilityFlags.SameType,
                Accessibility.Internal => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfFriendAssembly
                                                                      | AccessibilityFlags.AnyTypeOfFriendAssembly,
                Accessibility.Protected => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfAnyAssembly
                                                                       | AccessibilityFlags.DerivedTypeOfAnyAssembly,
                Accessibility.PrivateProtected => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfFriendAssembly,
                Accessibility.ProtectedInternal => AccessibilityFlags.SameType | AccessibilityFlags.AnyTypeOfFriendAssembly
                                                                               | AccessibilityFlags.DerivedTypeOfAnyAssembly,
                Accessibility.Public => AccessibilityFlags.Public,
                _ => throw new ArgumentOutOfRangeException( nameof(accessibility) )
            };
    }
}