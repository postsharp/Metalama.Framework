using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssign
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            _ = meta.Proceed();

            return default;
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}