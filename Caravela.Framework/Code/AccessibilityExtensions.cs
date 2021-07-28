// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class AccessibilityExtensions
    {
        public static SetRelationship CompareAccessibility( this Accessibility left, Accessibility right )
            => (left, right) switch
            {
                // Internal and protected are not comparable.
                (Accessibility.Internal, Accessibility.Protected ) => new SetRelationship( null ),
                (Accessibility.Protected, Accessibility.Internal ) => new SetRelationship( null ),
                _ => new SetRelationship( left - right ),
            };
    }
}