#if TESTOPTIONS
// @RequiredConstant(NET7_0_OR_GREATER)
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if NET7_0_OR_GREATER && ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Operators.InterfaceMembers_StaticVirtual
{
    public class Override : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Override.");
            return meta.Proceed();
        }
    }

    // <target>
    internal interface Interface
    {
        [Override]
        public static virtual Interface operator +(Interface a, Interface b)
        {
            return a;
        }
    }
}

#endif