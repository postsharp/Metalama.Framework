using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.CodeModel.TypedConstant_;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var expressionBuilder = new ExpressionBuilder();

        expressionBuilder.AppendExpression(new MyExpressionBuilder());
        expressionBuilder.AppendVerbatim("+");
        expressionBuilder.AppendExpression(GetExpression());
        expressionBuilder.AppendVerbatim("+");
        Append(expressionBuilder);

        return expressionBuilder.ToValue();
    }

    [CompileTime]
    object GetExpression() => TypedConstant.Create(42);

    void Append(ExpressionBuilder expressionBuilder) => expressionBuilder.AppendExpression((object)TypedConstant.Create(42));
}

[CompileTime]
class MyExpressionBuilder : IExpressionBuilder
{
    public IExpression ToExpression() => TypedConstant.Create(42);
}

class TargetCode
{
    // <target>
    [Aspect]
    int Method(int a)
    {
        return a;
    }
}