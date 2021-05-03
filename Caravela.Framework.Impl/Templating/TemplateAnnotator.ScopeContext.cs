// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private class ScopeContext
        {
            public static ScopeContext Default => new( SymbolDeclarationScope.Both, false, false, null );

            public SymbolDeclarationScope CurrentBreakOrContinueScope { get; }

            /// <summary>
            /// Gets a value indicating whether the current expression is obliged to be compile-time-only.
            /// </summary>
            public bool ForceCompileTimeOnlyExpression { get; }

            /// <summary>
            /// Gets a value indicating whether the current node is guarded by a conditional statement where the condition is a runtime-only
            /// expression.
            /// </summary>
            public bool IsRuntimeConditionalBlock { get; }

            public string? ForceCompileTimeOnlyExpressionReason { get; }

            public ScopeContext(
                SymbolDeclarationScope currentBreakOrContinueScope,
                bool isRuntimeConditionalBlock,
                bool forceCompileTimeOnlyExpression,
                string? forceCompileTimeOnlyExpressionReason )
            {
                this.CurrentBreakOrContinueScope = currentBreakOrContinueScope;
                this.IsRuntimeConditionalBlock = isRuntimeConditionalBlock;
                this.ForceCompileTimeOnlyExpression = forceCompileTimeOnlyExpression;
                this.ForceCompileTimeOnlyExpressionReason = forceCompileTimeOnlyExpressionReason;
            }
        }
    }
}