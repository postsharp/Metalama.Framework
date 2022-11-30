using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.UnassignedLocal
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            dynamic? result;
            result = meta.Proceed();
            return result;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}