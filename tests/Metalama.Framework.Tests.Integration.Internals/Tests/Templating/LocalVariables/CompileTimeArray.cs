using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.LocalVariables.CompileTimeArray
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int[] compileTime = meta.CompileTime( new int[10] );

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