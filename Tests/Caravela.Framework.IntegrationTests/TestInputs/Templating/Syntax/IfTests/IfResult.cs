#pragma warning disable CS8600, CS8603
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfResult
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            dynamic result = proceed();

            if (result == null)
            {
                return "";
            }

            return result;
        }
    }

    internal class TargetCode
    {
        private string Method(object a)
        {
            return a?.ToString();
        }
    }
}