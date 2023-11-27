#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - Default interface members need to be supported by the runtime.
#endif

#if NET5_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Indexers.InterfaceMembers
{
    /*
     * Tests overriding of interface non-abstract events.
     */

    internal class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.OverrideAccessors(
                builder.Target.Indexers.Single(),
                nameof(OverrideGet),
                nameof(OverrideSet) );
        }

        [Template]
        public dynamic? OverrideGet()
        {
            Console.WriteLine("Override.");
            return meta.Proceed();
        }

        [Template]
        public void OverrideSet()
        {
            Console.WriteLine("Override.");
            meta.Proceed();
        }
    }

    // <target>
    [Override]
    public interface Interface
    {
        public int this[int i]
        {
            get => 42;
            set { }
        }
    }

    // <target>
    public class TargetClass : Interface
    {
    }
}

#endif