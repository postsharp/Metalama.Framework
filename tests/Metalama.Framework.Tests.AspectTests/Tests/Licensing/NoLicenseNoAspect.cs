#if TEST_OPTIONS
// @LicenseKey(None)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.AspectTests.Tests.Licensing.NoLicenseNoAspect
{
    internal class Dummy
    {
        private void M() { }
    }

    internal class SomeAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }
}