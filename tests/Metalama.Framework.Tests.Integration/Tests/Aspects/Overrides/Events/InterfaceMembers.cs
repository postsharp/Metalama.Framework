#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - Default interface members need to be supported by the runtime.
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Events.InterfaceMembers
{
    /*
     * Tests overriding of interface non-abstract events.
     */

    internal class OverrideAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            Console.WriteLine("Override.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic value)
        {
            Console.WriteLine("Override.");
            meta.Proceed();
        }
    }

    // <target>
    public interface Interface
    {
#if NET5_0_OR_GREATER

        [Override]
        private event EventHandler PrivateEvent
        {
            add
            {
                Console.WriteLine("Original implementation");
            }

            remove
            {
                Console.WriteLine("Original implementation");
            }
        }

        [Override]
        public virtual event EventHandler VirtualEvent
        {
            add
            {
                Console.WriteLine("Original implementation");
            }

            remove
            {
                Console.WriteLine("Original implementation");
            }
        }

        [Override]
        public sealed event EventHandler SealedEvent
        {
            add
            {
                Console.WriteLine("Original implementation");
            }

            remove
            {
                Console.WriteLine("Original implementation");
            }
        }

        [Override]
        public static event EventHandler StaticEvent
        {
            add
            {
                Console.WriteLine("Original implementation");
            }

            remove
            {
                Console.WriteLine("Original implementation");
            }
        }
#endif
    }

    // <target>
    public class TargetClass : Interface
    {
    }
}
