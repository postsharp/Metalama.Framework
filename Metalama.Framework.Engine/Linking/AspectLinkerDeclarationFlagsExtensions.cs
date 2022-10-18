// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.Linking
{
    internal static class AspectLinkerDeclarationFlagsExtensions
    {
        public static bool HasFlagFast( this AspectLinkerDeclarationFlags value, AspectLinkerDeclarationFlags flags )
        {
            return (value & flags) == flags;
        }

        public static bool HasAnyFlagFast( this AspectLinkerDeclarationFlags value, AspectLinkerDeclarationFlags flags )
        {
            return (value & flags) != 0;
        }
    }
}