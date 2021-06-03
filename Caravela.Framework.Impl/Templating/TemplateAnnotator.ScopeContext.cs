// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private class ScopeContext
        {
            private readonly SymbolDeclarationScope _preferredScope;

            public static ScopeContext Default => new( SymbolDeclarationScope.Both, false, null, SymbolDeclarationScope.Both, null );

            public SymbolDeclarationScope CurrentBreakOrContinueScope { get; }

            /// <summary>
            /// Gets a value indicating whether the current expression is obliged to be compile-time-only.
            /// </summary>
            public bool ForceCompileTimeOnlyExpression => this._preferredScope == SymbolDeclarationScope.CompileTimeOnly;

            public bool PreferRunTimeExpression => this._preferredScope == SymbolDeclarationScope.RunTimeOnly;

            /// <summary>
            /// Gets a value indicating whether the current node is guarded by a conditional statement where the condition is a runtime-only
            /// expression.
            /// </summary>
            public bool IsRuntimeConditionalBlock { get; }

            public string? IsRuntimeConditionalBlockReason { get; }

            public string? PreferredScopeReason { get; }

            private ScopeContext(
                SymbolDeclarationScope currentBreakOrContinueScope,
                bool isRuntimeConditionalBlock,
                string? isRuntimeConditionalBlockReason,
                SymbolDeclarationScope preferredScope,
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
                => new( parentScope.CurrentBreakOrContinueScope, parentScope.IsRuntimeConditionalBlock, parentScope.IsRuntimeConditionalBlockReason,
                        SymbolDeclarationScope.CompileTimeOnly, reason );

            public static ScopeContext CreatePreferredRunTimeScope( ScopeContext parentScope, string reason )
                => new( parentScope.CurrentBreakOrContinueScope, parentScope.IsRuntimeConditionalBlock, parentScope.IsRuntimeConditionalBlockReason,
                        SymbolDeclarationScope.RunTimeOnly, reason );

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

            public static ScopeContext CreateBreakOrContinueScope( ScopeContext parentScope, SymbolDeclarationScope scope, string reason )
                => new( scope,
                        scope == SymbolDeclarationScope.RunTimeOnly || parentScope.IsRuntimeConditionalBlock,
                        reason,
                        parentScope._preferredScope,
                        parentScope.PreferredScopeReason );
        }
    }
}