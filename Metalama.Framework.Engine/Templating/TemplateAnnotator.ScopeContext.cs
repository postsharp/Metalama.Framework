// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateAnnotator
    {
        private sealed class ScopeContext
        {
            private readonly TemplatingScope _preferredScope;

            public static ScopeContext Default
                => new( TemplatingScope.RunTimeOrCompileTime, false, null, TemplatingScope.RunTimeOrCompileTime, null, false, null );

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

            public bool IsDynamicTypingForbidden { get; }

            private string? ForbidDynamicTypingReason { get; }

            private ScopeContext(
                TemplatingScope currentBreakOrContinueScope,
                bool isRuntimeConditionalBlock,
                string? isRuntimeConditionalBlockReason,
                TemplatingScope preferredScope,
                string? preferredScopeReason,
                bool isDynamicTypingForbidden,
                string? forbidDynamicTypingReason )
            {
                this.CurrentBreakOrContinueScope = currentBreakOrContinueScope;
                this.IsRuntimeConditionalBlock = isRuntimeConditionalBlock;
                this.IsRuntimeConditionalBlockReason = isRuntimeConditionalBlockReason;
                this._preferredScope = preferredScope;
                this.PreferredScopeReason = preferredScopeReason;
                this.IsDynamicTypingForbidden = isDynamicTypingForbidden;
                this.ForbidDynamicTypingReason = forbidDynamicTypingReason;
            }

            /// <summary>
            /// Enters an expression branch that must be compile-time because the parent must be
            /// compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public ScopeContext CompileTimeOnly( string reason )
                => new(
                    this.CurrentBreakOrContinueScope,
                    this.IsRuntimeConditionalBlock,
                    this.IsRuntimeConditionalBlockReason,
                    TemplatingScope.CompileTimeOnly,
                    reason,
                    this.IsDynamicTypingForbidden,
                    this.ForbidDynamicTypingReason );

            public ScopeContext RunTimeOrCompileTime( string reason )
                => new(
                    this.CurrentBreakOrContinueScope,
                    this.IsRuntimeConditionalBlock,
                    this.IsRuntimeConditionalBlockReason,
                    TemplatingScope.RunTimeOrCompileTime,
                    reason,
                    this.IsDynamicTypingForbidden,
                    this.ForbidDynamicTypingReason );

            public ScopeContext RunTimePreferred( string reason )
                => new(
                    this.CurrentBreakOrContinueScope,
                    this.IsRuntimeConditionalBlock,
                    this.IsRuntimeConditionalBlockReason,
                    TemplatingScope.RunTimeOnly,
                    reason,
                    this.IsDynamicTypingForbidden,
                    this.ForbidDynamicTypingReason );

            /// <summary>
            /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
            /// Local variables modified within such branch cannot be compile-time.
            /// </summary>
            public ScopeContext RunTimeConditional( string reason )
                => new(
                    this.CurrentBreakOrContinueScope,
                    true,
                    reason,
                    this._preferredScope,
                    this.PreferredScopeReason,
                    this.IsDynamicTypingForbidden,
                    this.ForbidDynamicTypingReason );

            public ScopeContext BreakOrContinue( TemplatingScope scope, string reason )
                => new(
                    scope,
                    scope == TemplatingScope.RunTimeOnly || this.IsRuntimeConditionalBlock,
                    scope == TemplatingScope.RunTimeOnly ? reason : this.IsRuntimeConditionalBlockReason,
                    this._preferredScope,
                    this.PreferredScopeReason,
                    this.IsDynamicTypingForbidden,
                    this.ForbidDynamicTypingReason );

            public ScopeContext ForbidDynamic( string reason )
                => new(
                    this.CurrentBreakOrContinueScope,
                    this.IsRuntimeConditionalBlock,
                    this.IsRuntimeConditionalBlockReason,
                    this._preferredScope,
                    this.PreferredScopeReason,
                    true,
                    reason );
        }
    }
}