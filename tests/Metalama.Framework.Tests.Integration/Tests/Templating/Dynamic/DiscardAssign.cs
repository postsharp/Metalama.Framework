using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssign
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            _ = meta.Proceed();
            
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