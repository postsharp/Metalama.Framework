// @LicenseExpression(None)

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.NoLicenseNoAspect
{
    class Dummy
    {
        private void M() { }
    }

    class SomeAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            throw new System.NotImplementedException();
        }
    }
}