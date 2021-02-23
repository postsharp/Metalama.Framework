using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachParameter
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a, int b)
        {
            return a + b;
        }
    }
}