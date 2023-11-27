#if TEST_OPTIONS
// @RequiredConstant(NET7_0_OR_GREATER) - Not available on .NET Framework
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if NET7_0_OR_GREATER && ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Events.InterfaceMembers_StaticVirtual
{
    /*
     * Tests overriding of interface static virtual methods.
     */

    internal class OverrideAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic? value)
        {
            Console.WriteLine("Override.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic? value)
        {
            Console.WriteLine("Override.");
            meta.Proceed();
        }
    }

    // <target>
    public interface Interface
    {
        [Override]
        public static virtual event EventHandler StaticVirtualEvent
        {
            add
            {
                Console.WriteLine("Original.");
            }

            remove
            {
                Console.WriteLine("Original.");
            }
        }
    }

    // <target>
    public class TargetClass : Interface
    {
    }
}

#endif