using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Misc.ThisError
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            // Must be an error.
            meta.Cast( meta.Target.Type, this );

            return 0;
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