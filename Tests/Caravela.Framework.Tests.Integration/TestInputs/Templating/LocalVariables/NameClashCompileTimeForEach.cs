using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashCompileTimeForEach
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            foreach (var p in target.Parameters)
            {
                string text = p.Name + " = " + p.Value;
                Console.WriteLine(text);
            }

            return proceed();
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}