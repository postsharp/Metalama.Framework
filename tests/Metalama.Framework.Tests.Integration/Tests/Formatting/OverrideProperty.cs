using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.OverrideProperty
{
    public class MyOverride : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed();
            set => meta.Proceed();
        }
    }
}