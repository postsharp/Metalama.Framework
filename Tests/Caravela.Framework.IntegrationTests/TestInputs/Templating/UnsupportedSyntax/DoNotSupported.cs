using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.DoNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < target.Parameters.Count);

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