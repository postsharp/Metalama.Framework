// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class TemplatingScopeExtensions
    {
        public static bool MustBeTransformed( this TemplatingScope scope )
            => scope.ReplaceIndeterminate( TemplatingScope.RunTimeOnly ) is
                TemplatingScope.RunTimeOnly or
                TemplatingScope.Dynamic;

        public static TemplatingScope DynamicToRunTimeOnly( this TemplatingScope scope )
            => scope == TemplatingScope.CompileTimeDynamic ? TemplatingScope.RunTimeOnly : scope;

        public static TemplatingScope DynamicToCompileTimeOnly( this TemplatingScope scope )
            => scope == TemplatingScope.CompileTimeDynamic ? TemplatingScope.CompileTimeOnly : scope;

        public static bool IsDynamic( this TemplatingScope scope ) => scope is TemplatingScope.CompileTimeDynamic or TemplatingScope.Dynamic;

        public static bool IsIndeterminate( this TemplatingScope scope ) => scope is TemplatingScope.Both or TemplatingScope.Unknown;

        public static bool IsRunTime( this TemplatingScope scope )
            => scope is TemplatingScope.Dynamic or TemplatingScope.CompileTimeDynamic or TemplatingScope.RunTimeOnly;

        
        public static TemplatingScope ReplaceIndeterminate( this TemplatingScope scope, TemplatingScope defaultScope )
            => scope.IsIndeterminate() ? defaultScope : scope;

        public static string ToDisplayString( this TemplatingScope scope )
            => scope switch
            {
                TemplatingScope.RunTimeOnly => "run-time",
                TemplatingScope.CompileTimeOnly => "compile-time",
                TemplatingScope.Both => "both",
                TemplatingScope.Unknown => "unknown",
                TemplatingScope.CompileTimeDynamic => "dynamic compile-time",
                TemplatingScope.Dynamic => "dynamic",

                // We also throw an exception for Dynamic because a caller should convert dynamic to run-time or compile-time according to the context.
                _ => throw new ArgumentOutOfRangeException( nameof(scope) )
            };
    }
}