using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.CompileTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            lock (meta.Compilation)
            {
                return meta.Proceed();
            }
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