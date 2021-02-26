using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachParameter
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = compileTime(0);
            foreach (var p in target.Parameters)
            {
                i++;
            }

            Console.WriteLine(i);

            dynamic result = proceed();
            return result;
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