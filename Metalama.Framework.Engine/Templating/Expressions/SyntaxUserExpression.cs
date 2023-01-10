// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="UserExpression"/> where the syntax is known upfront.
    /// </summary>
    internal class SyntaxUserExpression : UserExpression
    {
        public SyntaxUserExpression(
            ExpressionSyntax expression,
            IType type,
            bool isReferenceable = false,
            bool isAssignable = false )
        {
            _ = isReferenceable;

            this.Expression = expression;
            this.Type = type;
            this.IsAssignable = isAssignable;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => this.Expression;

        public override IType Type { get; }

        public override bool IsAssignable { get; }

        public ExpressionSyntax Expression { get; }

        protected override string ToStringCore() => this.Expression.ToString();
    }
}