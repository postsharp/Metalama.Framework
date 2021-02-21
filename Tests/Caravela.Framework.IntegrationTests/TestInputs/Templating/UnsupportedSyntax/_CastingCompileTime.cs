using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.CastingCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            string text = "";
            short c = (short)target.Parameters.Count;

            if (c > 0)
            {
                object s = target.Parameters[0].Name;
                if (s is string)
                {
                    text = (s as string) + "42";
                }
            }

            Console.WriteLine(text);

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        string Method(int a)
        {
            return a.ToString();
        }
    }
}