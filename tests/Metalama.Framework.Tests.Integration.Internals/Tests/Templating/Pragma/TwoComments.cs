using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.TwoComments
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.InsertComment("Oops 1", null, "Oops 2");
            meta.InsertComment("Oops 3", null, "Oops 4");
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