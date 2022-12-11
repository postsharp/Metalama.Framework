// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Templating
{
    internal partial class TemplateAnnotator
    {
        private sealed class ScopeContext
        {
            private readonly TemplatingScope _preferredScope;

            public static ScopeContext Default => new( TemplatingScope.RunTimeOrCompileTime, false, null, TemplatingScope.RunTimeOrCompileTime, null );

            public TemplatingScope CurrentBreakOrContinueScope { get; }

            /// <summary>
            /// Gets a value indicating whether the current expression is obliged to be compile-time-only.
            /// </summary>
            public bool ForceCompileTimeOnlyExpression => this._preferredScope == TemplatingScope.CompileTimeOnly;

            public bool PreferRunTimeExpression => this._preferredScope == TemplatingScope.RunTimeOnly;

            /// <summary>
            /// Gets a value indicating whether the current node is guarded by a conditional statement where the condition is a runtime-only
            /// expression.
            /// </summary>
            public bool IsRuntimeConditionalBlock { get; }

            public string? IsRuntimeConditionalBlockReason { get; }

            public string? PreferredScopeReason { get; }

            private ScopeContext(
                TemplatingScope currentBreakOrContinueScope,
                bool isRuntimeConditionalBlock,
                string? isRuntimeConditionalBlockReason,
                TemplatingScope preferredScope,
                string? preferredScopeReason )
            {
                this.CurrentBreakOrContinueScope = currentBreakOrContinueScope;
                this.IsRuntimeConditionalBlock = isRuntimeConditionalBlock;
                this.IsRuntimeConditionalBlockReason = isRuntimeConditionalBlockReason;
                this._preferredScope = preferredScope;
                this.PreferredScopeReason = preferredScopeReason;
            }

            /// <summary>
            /// Enters an expression branch that must be compile-time because the parent must be
            /// compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateForcedCompileTimeScope( ScopeContext parentScope, string reason )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    parentScope.IsRuntimeConditionalBlock,
                    parentScope.IsRuntimeConditionalBlockReason,
                    TemplatingScope.CompileTimeOnly,
                    reason );

            public static ScopeContext CreateForcedRunTimeScope( ScopeContext parentScope, string reason )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    parentScope.IsRuntimeConditionalBlock,
                    parentScope.IsRuntimeConditionalBlockReason,
                    TemplatingScope.RunTimeOnly,
                    reason );

            public static ScopeContext CreateRunTimeOrCompileTimeScope( ScopeContext parentScope, string reason )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    parentScope.IsRuntimeConditionalBlock,
                    parentScope.IsRuntimeConditionalBlockReason,
                    TemplatingScope.RunTimeOrCompileTime,
                    reason );

            public static ScopeContext CreatePreferredRunTimeScope( ScopeContext parentScope, string reason )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    parentScope.IsRuntimeConditionalBlock,
                    parentScope.IsRuntimeConditionalBlockReason,
                    TemplatingScope.RunTimeOnly,
                    reason );

            /// <summary>
            /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
            /// Local variables modified within such branch cannot be compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateRuntimeConditionalScope( ScopeContext parentScope, string reason )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    true,
                    reason,
                    parentScope._preferredScope,
                    parentScope.PreferredScopeReason );

            public static ScopeContext CreateBreakOrContinueScope( ScopeContext parentScope, TemplatingScope scope, string reason )
                => new(
                    scope,
                    scope == TemplatingScope.RunTimeOnly || parentScope.IsRuntimeConditionalBlock,
                    reason,
                    parentScope._preferredScope,
                    parentScope.PreferredScopeReason );
        }
    }
}