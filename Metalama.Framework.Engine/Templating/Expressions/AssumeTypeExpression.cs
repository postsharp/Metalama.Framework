// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxSerialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Templating.Expressions;

internal sealed class AssumeTypeExpression : UserExpression
{
    private readonly IExpression _expression;

    public AssumeTypeExpression( IExpression expression, IType type )
    {
        this._expression = expression;
        this.Type = type;
    }

    protected override ExpressionSyntax ToSyntax( SyntaxSerializationContext syntaxSerializationContext )
    {
        var expression = this._expression.ToExpressionSyntax( syntaxSerializationContext );
        
        var expressionWithNewTypeAnnotation = SymbolAnnotationMapper.AddExpressionTypeAnnotation( expression, this.Type.GetSymbol() );;

        return expressionWithNewTypeAnnotation;
    }

    public override IType Type { get; }
}