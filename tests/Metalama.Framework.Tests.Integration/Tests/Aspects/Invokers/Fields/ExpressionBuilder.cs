﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.ExpressionBuilder_;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        INamedType mappingType = (INamedType)meta.Target.Method.Parameters[0].Type;

        var from = meta.Target.Method.Parameters[0];
        var to = meta.Target.Method.Parameters[1];

        foreach (var fieldOrProperty in mappingType.FieldsAndProperties)
        {
            var eb = new ExpressionBuilder();
            eb.AppendExpression(fieldOrProperty.With(to));
            eb.AppendVerbatim(" = ");
            eb.AppendExpression(fieldOrProperty.With(from));
            meta.InsertStatement(eb.ToExpression());
        }

        return meta.Proceed();
    }
}

// <target>
internal class TargetClass
{
    public int F;
 
    [Test]
    public void Map(TargetClass source, TargetClass target) { }
}