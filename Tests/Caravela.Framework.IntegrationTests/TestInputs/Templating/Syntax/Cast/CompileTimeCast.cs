using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.Cast.CompileTimeCast
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            short c = (short)target.Parameters.Count;

            if (c > 0)
            {
                string text = compileTime("");
                object s = target.Parameters[0].Name;
                if (s is string)
                {
                    text = (s as string) + " = ";
                }

                Console.WriteLine(text + target.Parameters[0].Value);
            }

            return proceed();
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}