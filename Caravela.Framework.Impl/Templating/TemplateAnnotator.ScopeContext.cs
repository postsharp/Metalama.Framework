// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private class ScopeContext
        {
            public SymbolDeclarationScope CurrentScope { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the current expression is obliged to be compile-time-only.
            /// </summary>
            public bool ForceCompileTimeOnlyExpression { get; private set; }

            /// <summary>
            /// Gets a value indicating whether the current node is guarded by a conditional statement where the condition is a runtime-only
            /// expression.
            /// </summary>
            public bool IsRuntimeConditionalBlock { get; private set; }

            public string? ForceCompileTimeOnlyExpressionReason { get; private set; }

            private ScopeContext(
                SymbolDeclarationScope currentScope,
                bool isRuntimeConditionalBlock,
                bool forceCompileTimeOnlyExpression,
                string? forceCompileTimeOnlyExpressionReason )
            {
                this.CurrentScope = currentScope;
                this.IsRuntimeConditionalBlock = isRuntimeConditionalBlock;
                this.ForceCompileTimeOnlyExpression = forceCompileTimeOnlyExpression;
                this.ForceCompileTimeOnlyExpressionReason = forceCompileTimeOnlyExpressionReason;
            }

            /// <summary>
            /// Enters an expression branch that must be compile-time because the parent must be
            /// compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateForceCompileTimeExpression( string reason )
            {
                return new( SymbolDeclarationScope.CompileTimeOnly, false, true, reason );
            }

            /// <summary>
            /// Enters a branch of the syntax tree whose execution depends on a runtime-only condition.
            /// Local variables modified within such branch cannot be compile-time.
            /// </summary>
            /// <returns>A cookie to dispose at the end.</returns>
            public static ScopeContext CreateRuntimeConditionalBlock()
            {
                return new( SymbolDeclarationScope.RunTimeOnly, true, false, null );
            }

            public static ScopeContext CreateHelperScope(
                SymbolDeclarationScope scope,
                bool isRuntimeConditionalBlock = false,
                bool forceCompileTimeOnlyExpression = false,
                string? forceCompileTimeOnlyExpressionReason = null )
            {
                return new( scope, isRuntimeConditionalBlock, forceCompileTimeOnlyExpression, forceCompileTimeOnlyExpressionReason );
            }
        }
    }
}