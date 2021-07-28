// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    internal static class AccessibilityExtensions
    {
        public static AccessibilityComparison Compare( this Accessibility left, Accessibility right )
            => (left, right) switch
            {
                // Internal and protected are not comparable.
                (Accessibility.Internal, Accessibility.Protected) => new AccessibilityComparison( null ),
                (Accessibility.Protected, Accessibility.Internal) => new AccessibilityComparison( null ),
                _ => new AccessibilityComparison( left - right )
            };
    }
}