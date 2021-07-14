using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnObjectWithCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            object? x = meta.Parameters[0].Value;
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