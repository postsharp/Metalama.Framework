using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Cast.CompileTimeCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}