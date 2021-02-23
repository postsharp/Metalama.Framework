using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.ReturnStatement.ReturnObjectWithCast
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            object x = target.Parameters[0].Value;
            return x;
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}