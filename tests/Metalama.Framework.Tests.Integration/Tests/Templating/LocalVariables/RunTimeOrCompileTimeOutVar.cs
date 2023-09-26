using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.RunTimeOrCompileTimeOutVar;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        if (meta.Target.Parameters.Single().Value > 0)
        {
            var s = meta.CompileTime("0");
            int.TryParse(s, out var i);
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