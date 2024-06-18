using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Assign
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var x = TypeFactory.GetType( SpecialType.Int32 ).DefaultValue();

            x = meta.Proceed();
            x += meta.Proceed();
            x *= meta.Proceed();

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