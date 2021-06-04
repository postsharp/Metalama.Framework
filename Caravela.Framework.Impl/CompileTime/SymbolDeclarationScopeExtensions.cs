// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class SymbolDeclarationScopeExtensions
    {
        public static bool MustBeTransformed( this SymbolDeclarationScope scope )
            => scope.ReplaceDefault( SymbolDeclarationScope.RunTimeOnly ) is
                SymbolDeclarationScope.RunTimeOnly or
                SymbolDeclarationScope.Dynamic;

        public static SymbolDeclarationScope DynamicToRunTimeOnly( this SymbolDeclarationScope scope )
            => scope == SymbolDeclarationScope.CompileTimeDynamic ? SymbolDeclarationScope.RunTimeOnly : scope;

        public static SymbolDeclarationScope DynamicToCompileTimeOnly( this SymbolDeclarationScope scope )
            => scope == SymbolDeclarationScope.CompileTimeDynamic ? SymbolDeclarationScope.CompileTimeOnly : scope;

        public static bool IsDynamic( this SymbolDeclarationScope scope )
            => scope is SymbolDeclarationScope.CompileTimeDynamic or SymbolDeclarationScope.Dynamic;

        public static SymbolDeclarationScope ReplaceDefault( this SymbolDeclarationScope scope, SymbolDeclarationScope defaultScope )
            => scope == SymbolDeclarationScope.Both || scope == SymbolDeclarationScope.Unknown ? defaultScope : scope;

        public static string ToDisplayString( this SymbolDeclarationScope scope )
            => scope switch
            {
                SymbolDeclarationScope.RunTimeOnly => "run-time",
                SymbolDeclarationScope.CompileTimeOnly => "compile-time",
                SymbolDeclarationScope.Both => "both",
                SymbolDeclarationScope.Unknown => "unknown",
                SymbolDeclarationScope.CompileTimeDynamic => "dynamic compile-time",
                SymbolDeclarationScope.Dynamic => "dynamic",

                // We also throw an exception for Dynamic because a caller should convert dynamic to run-time or compile-time according to the context.
                _ => throw new ArgumentOutOfRangeException( nameof(scope) )
            };
    }
}