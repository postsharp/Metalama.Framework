using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.RunTimeInCompileTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            
            var i = meta.CompileTime(0);
            while (i < meta.Method.Name.Length)
            {
                i++;
                
                Console.WriteLine(i);
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