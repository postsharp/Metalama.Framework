using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.DynamicInLambda3;

#pragma warning disable CS0618 // Type or member is obsolete

class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        Console.WriteLine(string.Join(", ", meta.Target.Parameters.Select(p => (IExpression?)p.Value)));

        return meta.Proceed();
    }
}

class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}