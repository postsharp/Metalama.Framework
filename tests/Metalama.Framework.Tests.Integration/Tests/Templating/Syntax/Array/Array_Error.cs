using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Array.Array_Error
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            // Error: cannot mix build-time and run-time.
            var a = new object[] { meta.Target.Compilation, meta.This };
            return default;
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