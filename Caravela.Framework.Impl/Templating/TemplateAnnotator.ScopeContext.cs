// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private class ScopeContext
        {
            private readonly SymbolDeclarationScope _forcedScope;

            public static ScopeContext Default => new( SymbolDeclarationScope.Both, false, SymbolDeclarationScope.Both, null );

            public SymbolDeclarationScope CurrentBreakOrContinueScope { get; }

            /// <summary>
            /// Gets a value indicating whether the current expression is obliged to be compile-time-only.
            /// </summary>
            public bool ForceCompileTimeOnlyExpression => this._forcedScope == SymbolDeclarationScope.CompileTimeOnly;

            public bool PreferRunTimeExpression => this._forcedScope == SymbolDeclarationScope.RunTimeOnly;

            /// <summary>
            /// Gets a value indicating whether the current node is guarded by a conditional statement where the condition is a runtime-only
            /// expression.
            /// </summary>
            public bool IsRuntimeConditionalBlock { get; }

            public string? ForcedScopeReason { get; }

            public ScopeContext(
                SymbolDeclarationScope currentBreakOrContinueScope,
                bool isRuntimeConditionalBlock,
                SymbolDeclarationScope forcedScope,
                string? forcedScopeReason )
            {
                this.CurrentBreakOrContinueScope = currentBreakOrContinueScope;
                this.IsRuntimeConditionalBlock = isRuntimeConditionalBlock;
                this._forcedScope = forcedScope;
                this.ForcedScopeReason = forcedScopeReason;
            }

            /// <summary>
            /// Enters an expression branch that must be compile-time because the parent must be
            /// compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateForcedCompileTimeScope( ScopeContext parentScope, string reason )
                => new( parentScope.CurrentBreakOrContinueScope, parentScope.IsRuntimeConditionalBlock, SymbolDeclarationScope.CompileTimeOnly, reason );

            public static ScopeContext CreateForcedRunTimeScope( ScopeContext parentScope, string reason )
                => new( parentScope.CurrentBreakOrContinueScope, parentScope.IsRuntimeConditionalBlock, SymbolDeclarationScope.RunTimeOnly, reason );

            /// <summary>
            /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
            /// Local variables modified within such branch cannot be compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateRuntimeConditionalScope( ScopeContext parentScope )
                => new(
                    parentScope.CurrentBreakOrContinueScope,
                    true,
                    parentScope._forcedScope,
                    parentScope.ForcedScopeReason );

            public static ScopeContext CreateBreakOrContinueScope( ScopeContext parentScope, SymbolDeclarationScope scope )
                => new( scope,
                        scope == SymbolDeclarationScope.RunTimeOnly || parentScope.IsRuntimeConditionalBlock,
                        parentScope._forcedScope,
                        parentScope.ForcedScopeReason );
        }
    }
}