#pragma warning disable CS8600, CS8603
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.GotoNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            dynamic result = proceed();

            if (result != null) goto end;

            return default;

        end:
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