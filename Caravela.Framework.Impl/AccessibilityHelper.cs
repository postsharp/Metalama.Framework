using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl
{
    internal static class AccessibilityHelper
    {
        private static AccessibilityFlags ToAccessibilityFlags( this Accessibility accessibility )
            => accessibility switch
            {
                Accessibility.Private => AccessibilityFlags.Private,
                Accessibility.Internal => AccessibilityFlags.Internal,
                Accessibility.NotApplicable => AccessibilityFlags.None,
                Accessibility.Protected => AccessibilityFlags.Protected,
                Accessibility.ProtectedAndInternal => AccessibilityFlags.PrivateProtected,
                Accessibility.ProtectedOrInternal => AccessibilityFlags.ProtectedInternal,
                Accessibility.Public => AccessibilityFlags.Public,
                _ => throw new ArgumentOutOfRangeException( nameof(accessibility) )
            };

        public static AccessibilityFlags GetResultingAccessibility( this ISymbol symbol )
        {
            var accessibility = AccessibilityFlags.Public;

            for ( var s = symbol; s != null && s.DeclaredAccessibility != Accessibility.NotApplicable; s = s.ContainingSymbol )
            {
                accessibility &= symbol.DeclaredAccessibility.ToAccessibilityFlags();
            }

            return accessibility;
        }
    }
}