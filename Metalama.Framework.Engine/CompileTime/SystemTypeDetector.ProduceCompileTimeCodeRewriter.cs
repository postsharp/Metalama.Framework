// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class SystemTypeDetector
    {
        public static bool IsSystemType( INamedTypeSymbol namedType )
        {
            switch ( namedType.GetReflectionName() )
            {
                case "System.Index":
                    return true;
            }

            switch ( namedType.ContainingNamespace.ToDisplayString() )
            {
                case "System.Runtime.CompilerServices":
                case "System.Diagnostics.CodeAnalysis":
                    return true;
            }

            return false;
        }
    }
}