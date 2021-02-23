using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.LocalFuncNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            void LocalFunc(dynamic p)
            {
                Console.WriteLine(p.ToString());
            }

            dynamic result = proceed();

            LocalFunc(result);

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