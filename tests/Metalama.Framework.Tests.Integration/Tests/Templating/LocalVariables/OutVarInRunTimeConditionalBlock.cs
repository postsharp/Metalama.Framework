using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.OutVarInRunTimeConditionalBlock;

[CompileTime]
class Aspect
{
    void M(out int i, out int j) => (i, j) = (1, 2);

    [TestTemplate]
    dynamic? Template()
    {
        if (meta.Target.Parameters.Single().Value > 0)
        {
            int i = meta.CompileTime(0);
            M(out i, out var j);
            j++;
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