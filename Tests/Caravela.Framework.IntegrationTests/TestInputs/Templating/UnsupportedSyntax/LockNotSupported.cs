using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.LockNotSupported
{
    internal class Aspect
    {
        private static readonly object o = new object();

        [TestTemplate]
        private dynamic Template()
        {
            dynamic result;
            lock (o)
            {
                result = proceed();
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