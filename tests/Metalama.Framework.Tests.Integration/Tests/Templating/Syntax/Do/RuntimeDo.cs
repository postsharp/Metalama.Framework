using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Do.RunTimeDo;

class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        int i = 0;

        do
        {
            i++;
        }
        while (i < meta.Target.Parameters.Count);

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