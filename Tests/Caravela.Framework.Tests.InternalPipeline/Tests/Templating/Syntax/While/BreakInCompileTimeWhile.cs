using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.BreakInCompileTimeWhile
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
