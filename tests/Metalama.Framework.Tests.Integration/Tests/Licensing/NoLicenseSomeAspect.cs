// @LicenseExpression(None)

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.NoLicenseSomeAspect
{
    class Dummy
    {
        [SomeAspect]
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