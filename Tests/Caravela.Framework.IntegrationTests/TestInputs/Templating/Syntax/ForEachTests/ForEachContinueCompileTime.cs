using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachContinueCompileTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a, int bb)
        {
            return a + bb;
        }
    }
}