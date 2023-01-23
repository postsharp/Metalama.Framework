using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.While.RunTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int i = 0;
            while (i < meta.Target.Parameters.Count)
            {
                i++;
            }

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
}