using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using Xunit.Sdk;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.AppendExpressionNewException;

class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        foreach (var parameter in meta.Target.Parameters)
        {
            if (parameter.Value == null)
            {
                throw GetNewExceptionExpression(parameter.Name, $"Parameter {parameter} can't be null.").Value!;
            }
        }

        return meta.Proceed();
    }

    [CompileTime]
    private static IExpression GetNewExceptionExpression(string parameterName, string errorMessage)
        => ExpressionFactory.Capture(new ArgumentNullException(parameterName, errorMessage));
}

// <target>
class TargetClass
{
    [TestAspect]
    void M(object obj) { }
}
