using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfCompileTimeNested
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int t = compileTime(0);
            string name = target.Parameters[0].Name;
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
            dynamic result = proceed();
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