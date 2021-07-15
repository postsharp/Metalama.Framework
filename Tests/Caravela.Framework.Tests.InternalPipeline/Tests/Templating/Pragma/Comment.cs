using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.Comments
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.Comment("Oops 1", null, "Oops 2");
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