using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfCompileTimeNested
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int t = meta.CompileTime(0);
            string name = meta.Parameters[0].Name;
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
            dynamic result = meta.Proceed();
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