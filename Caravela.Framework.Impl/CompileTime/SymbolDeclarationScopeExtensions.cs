// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class SymbolDeclarationScopeExtensions
    {
        public static string ToDisplayString( this SymbolDeclarationScope scope )
         => scope switch
         {
             SymbolDeclarationScope.RunTimeOnly => "run-time",
             SymbolDeclarationScope.CompileTimeOnly => "compile-time",
             SymbolDeclarationScope.Template => "template",
             SymbolDeclarationScope.Default => "default",
             _ => throw new System.ArgumentOutOfRangeException(nameof(scope))
         };
    }
}