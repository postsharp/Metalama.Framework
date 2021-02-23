using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Proceed.AssignProceedWithManyReturns
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            dynamic result = proceed();
            return result;
        }
    }

    internal class TargetCode
    {
        private bool Method(int a)
        {
            if (a % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}