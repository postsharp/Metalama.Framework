using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;


namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicReceiverThisInStaticContext
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.This;
            
            return default;
        }
    }

    // <target>
    static class TargetCode
    {
        static int Method(int a)
        {
            return a;
        }
    }
}