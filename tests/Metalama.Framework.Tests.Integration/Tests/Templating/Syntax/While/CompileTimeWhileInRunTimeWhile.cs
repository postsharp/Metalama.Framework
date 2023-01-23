using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.While.CompileTimeWhileInRunTimeWhile
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
                int j = meta.CompileTime(4);
                while (j < 2)
                {
                    i++;
                }
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