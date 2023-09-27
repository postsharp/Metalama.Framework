using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.CompileTimeExpressionInRunTimeConditionalBlock;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        if (meta.Target.Parameters.Single().Value > 0)
        {
            int i = meta.CompileTime(0);
            int j = meta.CompileTime(0);
            (i, j) = (3, 4);
            Console.WriteLine($"i={i} j={j}");
        }

        return meta.Proceed();
    }
}

class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}