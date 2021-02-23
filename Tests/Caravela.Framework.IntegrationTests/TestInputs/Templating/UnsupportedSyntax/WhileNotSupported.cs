using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.WhileNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            int i = 0;
            while (i < target.Parameters.Count)
            {
                i++;
            }

            Console.WriteLine("Test result = " + i);

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