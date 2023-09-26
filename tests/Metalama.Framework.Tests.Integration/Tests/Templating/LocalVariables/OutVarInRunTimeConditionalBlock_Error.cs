using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.OutVarInRunTimeConditionalBlock_Error;

[CompileTime]
class Aspect
{
    void M(out int i) => i = 1;

    [TestTemplate]
    dynamic? Template()
    {
        int i = meta.CompileTime(0);

        if (meta.Target.Parameters.Single().Value > 0)
        {
            M(out i);
        }

        Console.WriteLine($"i={i}");

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