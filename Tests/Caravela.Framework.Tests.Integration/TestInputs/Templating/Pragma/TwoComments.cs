using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.TwoComments
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            pragma.Comment("Oops 1", null, "Oops 2");
            pragma.Comment("Oops 3", null, "Oops 4");
            return proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}