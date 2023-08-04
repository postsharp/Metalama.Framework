// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.SyntaxSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal sealed class CapturedUserExpression : UserExpression
{
    private readonly ICompilation _compilation;
    private readonly object? _expression;

    public CapturedUserExpression( ICompilation compilation, object? expression )
    {
        this._compilation = compilation;
        this._expression = expression;
    }

    public override IType Type => ((ICompilationInternal) this._compilation).Factory.GetSpecialType( SpecialType.Object );

    protected override ExpressionSyntax ToSyntax( SyntaxSerializationContext syntaxSerializationContext )
        => TypedExpressionSyntaxImpl.FromValue( this._expression, syntaxSerializationContext ).Syntax;
}