using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.CompileTimeWhileInRunTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = 0;
            while (i < meta.Parameters.Count)
            {
                i++;
                int j = meta.CompileTime(0);
                while (j < 2)
                {
                    i++;
// The following line is not allowed because                    
//                    j++;
                }
            }

            Console.WriteLine("Test result = " + i);

            dynamic result = meta.Proceed();
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