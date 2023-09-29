using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.CompileTimeVariableInRunTimeConditionalBlock;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        if (meta.Target.Parameters.Single().Value > 0)
        {
            int i = meta.CompileTime(0);
            i++;
            i += 1;
            i = i + 1;
            Console.WriteLine($"i={i}");
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