// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class TemplatingScopeExtensions
    {
        public static bool MustBeTransformed( this TemplatingScope scope )
            => scope.GetExpressionExecutionScope().ReplaceIndeterminate( TemplatingScope.RunTimeOnly ) is
                TemplatingScope.RunTimeOnly;

        public static bool IsCompileTimeMemberReturningRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.Dynamic;

        public static bool EvaluatesToRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.Dynamic or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.RunTimeOnly;

        public static bool ExecutesAtCompileTimeOnly( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnly or TemplatingScope.CompileTimeOnlyReturningBoth or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
                or TemplatingScope.Dynamic;

        public static TemplatingScope ReplaceIndeterminate( this TemplatingScope scope, TemplatingScope defaultScope )
            => IsUndetermined( scope ) ? defaultScope : scope;

        public static bool IsUndetermined( this TemplatingScope scope ) => scope == TemplatingScope.RunTimeOrCompileTime || scope == TemplatingScope.Unknown;

        public static TemplatingScope GetExpressionExecutionScope( this TemplatingScope scope )
            => scope switch
            {
                TemplatingScope.CompileTimeOnlyReturningBoth => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => TemplatingScope.CompileTimeOnly,
                TemplatingScope.Dynamic => TemplatingScope.RunTimeOnly,
                _ => scope
            };

        public static TemplatingScope GetExpressionValueScope( this TemplatingScope scope, bool preferCompileTime = false )
            => scope switch
            {
                TemplatingScope.CompileTimeOnlyReturningBoth when preferCompileTime => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningBoth when !preferCompileTime => TemplatingScope.RunTimeOrCompileTime,
                TemplatingScope.Dynamic => TemplatingScope.RunTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => TemplatingScope.RunTimeOnly,
                _ => scope
            };

        public static string ToDisplayString( this TemplatingScope scope )
            => scope switch
            {
                TemplatingScope.RunTimeOnly => "run-time",
                TemplatingScope.CompileTimeOnly => "compile-time",
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => "compile-time",
                TemplatingScope.CompileTimeOnlyReturningBoth => "compile-time",
                TemplatingScope.RunTimeOrCompileTime => "both",
                TemplatingScope.Unknown => "unknown",
                TemplatingScope.Dynamic => "dynamic",

                // We also throw an exception for Dynamic because a caller should convert dynamic to run-time or compile-time according to the context.
                _ => throw new ArgumentOutOfRangeException( nameof(scope) )
            };
    }
}