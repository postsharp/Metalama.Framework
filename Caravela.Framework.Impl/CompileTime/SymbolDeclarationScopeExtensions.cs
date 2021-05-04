// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class SymbolDeclarationScopeExtensions
    {
        public static bool IsRunTime( this SymbolDeclarationScope scope ) => scope is SymbolDeclarationScope.Both or SymbolDeclarationScope.RunTimeOnly;

        public static bool IsCompileTime( this SymbolDeclarationScope scope ) => scope is SymbolDeclarationScope.Both or SymbolDeclarationScope.CompileTimeOnly;

        public static bool MustBeTransformed( this SymbolDeclarationScope scope )
            => scope.ReplaceDefault( SymbolDeclarationScope.RunTimeOnly ) == SymbolDeclarationScope.RunTimeOnly;

        public static SymbolDeclarationScope DynamicToRunTimeOnly( this SymbolDeclarationScope scope )
            => scope == SymbolDeclarationScope.Dynamic ? SymbolDeclarationScope.RunTimeOnly : scope;
        
        public static SymbolDeclarationScope DynamicToCompileTimeOnly( this SymbolDeclarationScope scope )
            => scope == SymbolDeclarationScope.Dynamic ? SymbolDeclarationScope.CompileTimeOnly : scope;

        public static SymbolDeclarationScope ReplaceDefault( this SymbolDeclarationScope scope, SymbolDeclarationScope defaultScope )
            => scope == SymbolDeclarationScope.Both || scope == SymbolDeclarationScope.Unknown ? defaultScope : scope;

        public static string ToDisplayString( this SymbolDeclarationScope scope )
            => scope switch
            {
                SymbolDeclarationScope.RunTimeOnly => "run-time",
                SymbolDeclarationScope.CompileTimeOnly => "compile-time",
                SymbolDeclarationScope.Both => "both",
                SymbolDeclarationScope.Unknown => "unknown",
                
                // We also throw an exception for Dynamic because a caller should convert dynamic to run-time or compile-time according to the context.
                _ => throw new ArgumentOutOfRangeException( nameof(scope) )
            };
    }
}