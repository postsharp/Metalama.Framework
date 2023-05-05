using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Do.BreakInCompileTimeDo;

class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {        
        var i = meta.CompileTime(0);

        do
        {
            i++;

            if (i > 4)
            {
                break;
            }
        }
        while (i < meta.Target.Method.Name.Length);

        Console.WriteLine("Test result = " + i);

        dynamic? result = meta.Proceed();
        return result;
    }
}

class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
