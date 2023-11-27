#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - Default interface members need to be supported by the runtime.
#endif

#if NET5_0_OR_GREATER
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.InterfaceMembers
{
    /*
     * Tests overriding of interface non-abstract properties.
     */

    internal class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                Console.WriteLine("Override.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Override.");
                meta.Proceed();
            }
        }
    }

    // <target>
    public interface Interface
    {

        [Override]
        private int PrivateProperty
        {
            get
            {
                Console.WriteLine("Original implementation");
                return 42;
            }

            set
            {
                Console.WriteLine("Original implementation");
            }
        }

        [Override]
        public static int StaticAutoProperty { get; set; }

        [Override]
        public static int StaticProperty
        {
            get
            {
                Console.WriteLine("Original implementation");
                return 42;
            }

            set
            {
                Console.WriteLine("Original implementation");
            }
        }
    }

    // <target>
    public class TargetClass : Interface
    {
    }
}
#endif
