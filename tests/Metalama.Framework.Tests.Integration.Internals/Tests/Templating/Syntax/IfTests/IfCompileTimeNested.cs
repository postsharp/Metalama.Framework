using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfCompileTimeNested
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int t = meta.CompileTime(0);
            string name = meta.Target.Parameters[0].Name;
            if (name.Contains("a"))
            {
                if (name.Contains("b"))
                {
                    t = 1;
                }
                else
                {
                    if (name.Contains("c"))
                    {
                        t = 42;
                    }
                    else
                    {
                        t = 3;
                    }
                }
            }
            else
            {
                t = 4;
            }

            Console.WriteLine(t);
            dynamic? result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        void Method(string aBc)
        {
        }
    }
}