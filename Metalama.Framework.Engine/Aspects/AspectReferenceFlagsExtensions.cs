// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Aspects
{
    internal static class AspectReferenceFlagsExtensions
    {
        public static bool HasFlagFast( this AspectReferenceFlags value, AspectReferenceFlags flags )
    {
        return (value & flags) == flags;
    }
}
}