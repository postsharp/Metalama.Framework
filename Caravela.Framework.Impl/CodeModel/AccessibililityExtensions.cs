// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class AccessibililityExtensions
    {
        public static bool IsMoreRestrictiveThan( this Accessibility left, Accessibility right )
            => left switch
            {
                // Internal and protected are not comparable.
                Accessibility.Internal =>
                    right != Accessibility.Protected && left < right,
                Accessibility.Protected =>
                    right != Accessibility.Internal && left < right,
                _ => left < right
            };

        public static bool IsMoreRestrictiveOrEqualThan( this Accessibility left, Accessibility right )
            => left switch
            {
                // Internal and protected are not comparable.
                Accessibility.Internal =>
                    right != Accessibility.Protected && left <= right,
                Accessibility.Protected =>
                    right != Accessibility.Internal && left <= right,
                _ => left <= right
            };
    }
}