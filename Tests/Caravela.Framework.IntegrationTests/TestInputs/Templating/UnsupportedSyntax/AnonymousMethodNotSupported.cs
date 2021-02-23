using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.AnonymousMethodNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            Action<object> action =
            delegate (object p)
            {
                Console.WriteLine(p.ToString());
            };

            dynamic result = proceed();

            action(result);

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