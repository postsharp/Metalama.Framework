using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

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
        // <target>
        int Method(int a)
        {
            return a;
        }
    }
}