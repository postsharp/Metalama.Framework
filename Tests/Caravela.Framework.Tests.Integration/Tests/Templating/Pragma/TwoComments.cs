using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.TwoComments
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.Comment("Oops 1", null, "Oops 2");
            meta.Comment("Oops 3", null, "Oops 4");
            return meta.Proceed();
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