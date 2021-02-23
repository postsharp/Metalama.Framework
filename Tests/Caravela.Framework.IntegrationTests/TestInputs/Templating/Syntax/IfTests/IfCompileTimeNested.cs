using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfCompileTimeNested
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private void Method(string aBc)
        {
        }
    }
}