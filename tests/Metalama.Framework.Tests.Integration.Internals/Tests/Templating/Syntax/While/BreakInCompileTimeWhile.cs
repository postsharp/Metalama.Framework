using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.While.BreakInCompileTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            
            var i = meta.CompileTime(0);
            while (i < meta.Target.Method.Name.Length)
            {
                i++;
                
                if (i > 4)
                {
                    break;
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
