#pragma warning disable CS8600, CS8603
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.ReturnStatement.ReturnDefault
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            try
            {
                dynamic result = proceed();
                return result;
            }
            catch
            {
                return default;
            }
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return 42 / a;
        }
    }
}