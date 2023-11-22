using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Assign
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            dynamic? x = TypeFactory.GetType(SpecialType.Int32).DefaultValue();

            x = meta.Proceed();
            x += meta.Proceed();
            x *= meta.Proceed();

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