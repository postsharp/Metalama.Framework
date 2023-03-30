// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal sealed class DefaultUserExpression : UserExpression
    {
        public DefaultUserExpression( IType type )
        {
            this.Type = type;
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var typeSymbol = this.Type.GetSymbol();

            return syntaxGenerationContext.SyntaxGenerator.DefaultExpression( typeSymbol );
        }

        public override IType Type { get; }
    }
}