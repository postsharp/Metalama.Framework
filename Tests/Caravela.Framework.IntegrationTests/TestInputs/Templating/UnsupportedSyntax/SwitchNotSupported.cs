#pragma warning disable CS8600, CS8603
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.SwitchNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            dynamic result;
            switch (target.Parameters.Count)
            {
                case 0:
                    result = null;
                    break;
                case 1:
                    result = target.Parameters[0].Value;
                    break;
                case 2:
                    goto default;
                case 3:
                    goto case 2;
                default:
                    result = proceed();
                    break;
            }
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