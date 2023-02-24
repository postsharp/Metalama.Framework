using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.TypedConstantArgument;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        if (meta.Target.Parameters[0].Value == 0)
        {
            var expr = ExpressionFactory.Capture(TypedConstant.Create(42));
            return meta.Target.Method.Invoke(TypedConstant.Create(42), expr);
        }
        else
            return meta.Proceed();
    }
}

// <target>
internal class TargetClass
{
    [Test]
    void M(int i, int j) => Console.WriteLine(i + j);
}