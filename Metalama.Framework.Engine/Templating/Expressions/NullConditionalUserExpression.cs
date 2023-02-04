// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

/// <summary>
/// A wrapper of any <see cref="UserExpression"/> that means that null-conditional member access should be generated.
/// </summary>
internal sealed class NullConditionalUserExpression : UserExpression
{
    public UserExpression UnderlyingExpression { get; }

    public NullConditionalUserExpression( UserExpression expression )
    {
        this.UnderlyingExpression = expression;
    }

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        => this.UnderlyingExpression.ToSyntax( syntaxGenerationContext );

    public override IType Type => this.UnderlyingExpression.Type.ToNullableType();
}