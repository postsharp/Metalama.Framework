// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class TemplatingScopeExtensions
    {
        public static bool MustBeTransformed( this TemplatingScope scope )
            => scope != TemplatingScope.RunTimeTemplateParameter && scope.GetExpressionExecutionScope().ReplaceIndeterminate( TemplatingScope.RunTimeOnly ) is
                TemplatingScope.RunTimeOnly;

        public static bool IsCompileTimeMemberReturningRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.Dynamic or TemplatingScope.RunTimeTemplateParameter;

        public static bool EvaluatesToRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.Dynamic or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.RunTimeOnly;

        public static bool MustExecuteAtCompileTime( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnly or TemplatingScope.CompileTimeOnlyReturningBoth or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
                or TemplatingScope.Dynamic;

        public static bool CanExecuteAtCompileTime( this TemplatingScope scope )
            => scope.MustExecuteAtCompileTime() || scope == TemplatingScope.RunTimeOrCompileTime;

        public static TemplatingScope ReplaceIndeterminate( this TemplatingScope scope, TemplatingScope defaultScope )
            => IsUndetermined( scope ) ? defaultScope : scope;

        public static bool IsUndetermined( this TemplatingScope scope ) => scope == TemplatingScope.RunTimeOrCompileTime || scope == TemplatingScope.Unknown;

        public static TemplatingScope GetExpressionExecutionScope( this TemplatingScope scope )
            => scope switch
            {
                TemplatingScope.CompileTimeOnlyReturningBoth => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => TemplatingScope.CompileTimeOnly,
                TemplatingScope.Dynamic => TemplatingScope.RunTimeOnly,
                TemplatingScope.RunTimeTemplateParameter => TemplatingScope.RunTimeOnly,
                _ => scope
            };

        public static TemplatingScope GetExpressionValueScope( this TemplatingScope scope, bool preferCompileTime = false )
            => scope switch
            {
                TemplatingScope.CompileTimeOnlyReturningBoth when preferCompileTime => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningBoth when !preferCompileTime => TemplatingScope.RunTimeOrCompileTime,
                TemplatingScope.Dynamic => TemplatingScope.RunTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => TemplatingScope.RunTimeOnly,
                TemplatingScope.RunTimeTemplateParameter => TemplatingScope.RunTimeOnly,
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

        public static TemplatingScope CombineScopes( TemplatingScope executionScope, TemplatingScope valueScope )
            => (executionScope.GetExpressionExecutionScope(), valueScope.GetExpressionValueScope()) switch
            {
                (TemplatingScope.CompileTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.CompileTimeOnlyReturningRuntimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.CompileTimeOnlyReturningBoth,
                (TemplatingScope.RunTimeOnly, _) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOrCompileTime,
                
                // Unknown scopes happen in dynamic code that cannot be resolved to symbols.
                (TemplatingScope.Unknown, _) => TemplatingScope.RunTimeOnly,
                (_, TemplatingScope.Unknown) => TemplatingScope.RunTimeOnly,
                _ => throw new AssertionFailedException( $"Invalid combination: {executionScope}, {valueScope}." )
            };
    }
}
