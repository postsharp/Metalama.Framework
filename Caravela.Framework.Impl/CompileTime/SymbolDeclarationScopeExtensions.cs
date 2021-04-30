// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class SymbolDeclarationScopeExtensions
    {
        public static bool IsRunTime( this SymbolDeclarationScope scope ) => scope is SymbolDeclarationScope.Both or SymbolDeclarationScope.RunTimeOnly;

        public static bool IsCompileTime( this SymbolDeclarationScope scope ) => scope is SymbolDeclarationScope.Both or SymbolDeclarationScope.CompileTimeOnly;

        public static SymbolDeclarationScope ReplaceDefault( this SymbolDeclarationScope scope, SymbolDeclarationScope defaultScope )
            => scope == SymbolDeclarationScope.Both ? defaultScope : scope;

        public static string ToDisplayString( this SymbolDeclarationScope scope )
            => scope switch
            {
                SymbolDeclarationScope.RunTimeOnly => "run-time",
                SymbolDeclarationScope.CompileTimeOnly => "compile-time",
                SymbolDeclarationScope.Both => "both",
                SymbolDeclarationScope.Unknown => "unknown",
                _ => throw new ArgumentOutOfRangeException( nameof(scope) )
            };
    }
}