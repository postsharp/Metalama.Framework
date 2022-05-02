// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Code
{
    public static class AccessibilityExtensions
    {
        private static bool IsSupersetOf( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == right && left != right;

        private static bool IsSubsetOf( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == left && left != right;

        private static bool IsSupersetOrEqual( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == right;

        private static bool IsSubsetOrEqual( this AccessibilityFlags left, AccessibilityFlags right ) => (right & left) == left;

        private static AccessibilityFlags ToAccessibilityFlags( this Accessibility accessibility )
            => accessibility switch
            {
                Accessibility.Private => AccessibilityFlags.SameType,
                Accessibility.Internal => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfFriendAssembly
                                                                      | AccessibilityFlags.AnyTypeOfFriendAssembly,
                Accessibility.Protected => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfFriendAssembly
                                                                       | AccessibilityFlags.DerivedTypeOfAnyAssembly,
                Accessibility.PrivateProtected => AccessibilityFlags.SameType | AccessibilityFlags.DerivedTypeOfFriendAssembly,
                Accessibility.ProtectedInternal => AccessibilityFlags.SameType | AccessibilityFlags.AnyTypeOfFriendAssembly
                                                                               | AccessibilityFlags.DerivedTypeOfFriendAssembly
                                                                               | AccessibilityFlags.DerivedTypeOfAnyAssembly,
                Accessibility.Public => AccessibilityFlags.Public,
                _ => throw new ArgumentOutOfRangeException( nameof(accessibility) )
            };

        public static bool IsSupersetOf( this Accessibility left, Accessibility right )
            => left.ToAccessibilityFlags().IsSupersetOf( right.ToAccessibilityFlags() );

        public static bool IsSubsetOf( this Accessibility left, Accessibility right ) => left.ToAccessibilityFlags().IsSubsetOf( right.ToAccessibilityFlags() );

        public static bool IsSupersetOrEqual( this Accessibility left, Accessibility right )
            => left.ToAccessibilityFlags().IsSupersetOrEqual( right.ToAccessibilityFlags() );

        public static bool IsSubsetOrEqual( this Accessibility left, Accessibility right )
            => left.ToAccessibilityFlags().IsSubsetOrEqual( right.ToAccessibilityFlags() );
    }
}