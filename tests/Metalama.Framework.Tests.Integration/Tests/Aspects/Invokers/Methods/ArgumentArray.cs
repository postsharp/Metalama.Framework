﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.ArgumentArray;

#pragma warning disable CS0618 // Select is obsolete

public class TestAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceMethod(builder.Target.DeclaringType, nameof(GetMethodInvokerDelegate), args: new { method = builder.Target });
    }

    [Template]
    public Func<object?, object?[], object?> GetMethodInvokerDelegate(IMethod method)
    {
        return Invoke;

        object? Invoke(object? instance, object?[] args)
        {
            var argExpressions = method.Parameters.Select(p => ExpressionFactory.Capture(args[p.Index]!));

            return method.Invoke(argExpressions);
        }
    }
}

// <target>
internal class TargetClass
{
    [Test]
    int M(int i, int j) => i + j;
}