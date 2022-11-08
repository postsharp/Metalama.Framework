// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CompileTime
{
    internal static class TemplatingScopeExtensions
    {
        public static bool MustBeTransformed( this TemplatingScope scope )
            => scope != TemplatingScope.RunTimeTemplateParameter && scope.GetExpressionExecutionScope() is TemplatingScope.RunTimeOnly;

        public static bool IsCompileTimeMemberReturningRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.Dynamic or TemplatingScope.RunTimeTemplateParameter;

        public static bool EvaluatesToRunTimeValue( this TemplatingScope scope )
            => scope is TemplatingScope.Dynamic or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly or TemplatingScope.RunTimeOnly;

        public static bool MustExecuteAtCompileTime( this TemplatingScope scope )
            => scope is TemplatingScope.CompileTimeOnly or TemplatingScope.CompileTimeOnlyReturningBoth or TemplatingScope.CompileTimeOnlyReturningRuntimeOnly
                or TemplatingScope.Dynamic or TemplatingScope.TypeOfRunTimeType or TemplatingScope.TypeOfTemplateTypeParameter;

        public static bool CanExecuteAtCompileTime( this TemplatingScope scope )
            => scope.MustExecuteAtCompileTime() || scope == TemplatingScope.RunTimeOrCompileTime;

        public static TemplatingScope ReplaceIndeterminate( this TemplatingScope scope, TemplatingScope defaultScope )
            => IsUndetermined( scope ) ? defaultScope : scope;

        public static bool IsUndetermined( this TemplatingScope scope )
            => scope is TemplatingScope.RunTimeOrCompileTime or TemplatingScope.LateBound or TemplatingScope.MustFollowParent;

        public static TemplatingScope GetExpressionExecutionScope( this TemplatingScope scope, bool preferCompileTime = false )
            => scope switch
            {
                TemplatingScope.CompileTimeOnlyReturningBoth => TemplatingScope.CompileTimeOnly,
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => TemplatingScope.CompileTimeOnly,
                TemplatingScope.Dynamic => TemplatingScope.RunTimeOnly,
                TemplatingScope.RunTimeTemplateParameter => TemplatingScope.RunTimeOnly,
                TemplatingScope.TypeOfRunTimeType => TemplatingScope.RunTimeOrCompileTime,
                TemplatingScope.TypeOfTemplateTypeParameter => TemplatingScope.RunTimeOnly,
                TemplatingScope.RunTimeOrCompileTime when preferCompileTime => TemplatingScope.CompileTimeOnly,
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
                TemplatingScope.TypeOfRunTimeType => TemplatingScope.RunTimeOrCompileTime,
                TemplatingScope.TypeOfTemplateTypeParameter => TemplatingScope.RunTimeOnly,
                _ => scope
            };

        public static string ToDisplayString( this TemplatingScope scope )
            => scope switch
            {
                TemplatingScope.RunTimeOnly => "run-time",
                TemplatingScope.CompileTimeOnly => "compile-time",
                TemplatingScope.CompileTimeOnlyReturningRuntimeOnly => "compile-time-returning-run-time",
                TemplatingScope.CompileTimeOnlyReturningBoth => "compile-time",
                TemplatingScope.RunTimeOrCompileTime => "run-time-or-compile-time",
                TemplatingScope.TypeOfRunTimeType => "run-time-or-compile-time",
                TemplatingScope.TypeOfTemplateTypeParameter => "run-time",
                TemplatingScope.LateBound => "unbound",
                TemplatingScope.Dynamic => "run-time",
                TemplatingScope.Invalid => "invalid",

                _ => scope.ToString()
            };

        public static TemplatingScope GetAccessMemberScope( TemplatingScope executionScope, TemplatingScope valueScope )
            => (executionScope.GetExpressionExecutionScope(), valueScope.GetExpressionValueScope()) switch
            {
                (TemplatingScope.CompileTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.CompileTimeOnlyReturningRuntimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.CompileTimeOnlyReturningBoth,
                (TemplatingScope.RunTimeOnly, _) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOrCompileTime,
                (TemplatingScope.TypeOfRunTimeType, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.TypeOfTemplateTypeParameter, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOnly,

                // Unknown scopes happen in dynamic code that cannot be resolved to symbols.
                (TemplatingScope.LateBound, _) => valueScope,

                // Conflicts are ignored. They should be reported elsewhere.
                (_, TemplatingScope.Conflict) => executionScope,

                _ => throw new AssertionFailedException( $"Invalid combination: {executionScope}, {valueScope}." )
            };

        public static TemplatingScope GetCombinedExecutionScope( this TemplatingScope a, TemplatingScope b ) => a.GetCombinedScope( b, true );

        public static TemplatingScope GetCombinedValueScope( this TemplatingScope a, TemplatingScope b )
        {
            return a.GetCombinedScope( b, false );
        }

        private static TemplatingScope GetCombinedScope( this TemplatingScope a, TemplatingScope b, bool isExecutionScope )
            => (a, b) switch
            {
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOrCompileTime,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.CompileTimeOnly,

                // Propagate conflicts.
                (TemplatingScope.Conflict, _) => TemplatingScope.Conflict,
                (_, TemplatingScope.Conflict) => TemplatingScope.Conflict,

                // Do not propagate the error down. It should be reported in child nodes.
                (_, TemplatingScope.Invalid) => a,

                // If any part of an expression is late bound, the whole expression is also.
                (_, TemplatingScope.LateBound) => TemplatingScope.LateBound,
                (TemplatingScope.LateBound, _) => TemplatingScope.LateBound,

                (TemplatingScope.Invalid, _) => TemplatingScope.Invalid, // This happens when the expression itself is invalid, not a child.  
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.CompileTimeOnly) => TemplatingScope.CompileTimeOnly,
                (TemplatingScope.RunTimeOrCompileTime, TemplatingScope.RunTimeOnly) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOnly, TemplatingScope.RunTimeOrCompileTime) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOnly, TemplatingScope.CompileTimeOnly) when isExecutionScope => TemplatingScope.RunTimeOnly,
                (TemplatingScope.RunTimeOnly, TemplatingScope.CompileTimeOnly) => TemplatingScope.Conflict,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) when isExecutionScope => TemplatingScope.RunTimeOnly,
                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOnly) => TemplatingScope.Conflict,

                _ => throw new AssertionFailedException( $"Invalid combination: {a}, {b}." )
            };
    }
}