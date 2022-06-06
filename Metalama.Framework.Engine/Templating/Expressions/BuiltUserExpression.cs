// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="UserExpression"/> where the syntax is known upfront.
    /// </summary>
    internal class BuiltUserExpression : UserExpression
    {
        private readonly ExpressionSyntax _expression;

        public BuiltUserExpression(
            ExpressionSyntax expression,
            IType type,
            bool isReferenceable = false,
            bool isAssignable = false )
        {
            _ = isReferenceable;

            this._expression = expression;
            this.Type = type;
            this.IsAssignable = isAssignable;
        }

        public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => this._expression;

        public override IType Type { get; }

        public override bool IsAssignable { get; }
    }
}