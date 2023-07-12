using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.While.RunTimeOrCompileTimeWhile;

class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        int i = 0;
        while (true)
        {
            i++;

            if (i >= meta.Target.Parameters.Count)
                break;
        }

        Console.WriteLine("Test result = " + i);

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