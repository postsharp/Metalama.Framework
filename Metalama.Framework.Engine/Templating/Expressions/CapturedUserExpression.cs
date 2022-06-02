// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
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

    public override IType Type => ((ICompilationInternal) this._compilation).TypeFactory.GetSpecialType( SpecialType.Object );

    public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        => RunTimeTemplateExpression.FromValue( this._expression, this._compilation, syntaxGenerationContext ).Syntax;
}