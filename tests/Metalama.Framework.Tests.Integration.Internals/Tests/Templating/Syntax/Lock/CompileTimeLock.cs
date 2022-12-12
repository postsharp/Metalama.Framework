using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lock.CompileTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            lock (meta.Target.Compilation)
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