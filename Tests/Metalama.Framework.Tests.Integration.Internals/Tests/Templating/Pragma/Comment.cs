using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.Comments
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.InsertComment("Oops 1", null, "Oops 2", "Multi\nline");
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