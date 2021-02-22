using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachContinueCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = compileTime(0);
            foreach (var p in target.Parameters)
            {
                if (p.Name.Length <= 1) continue;
                i++;
            }

            Console.WriteLine(i);

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int bb)
        {
            return a + bb;
        }
    }
}