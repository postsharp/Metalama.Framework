#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.InheritedMethodLevel
{
    [Inherited]
    internal class InheritedAspectAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Hacked!");
            return meta.Proceed();
        }
    }
}