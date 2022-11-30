using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnObjectWithCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            object? x = meta.Target.Parameters[0].Value;
            return x;
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