using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Proceed.EventHandler_NP_P
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect1")]
        event EventHandler Foo_Override1
        {
            add
            {
                Console.WriteLine("Override1 Start");
                link[_this.Foo, inline] += value;
                Console.WriteLine("Override1 End");
            }
            remove
            {
                Console.WriteLine("Override1 Start");
                link[_this.Foo, inline] -= value;
                Console.WriteLine("Override1 End");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        event EventHandler Foo_Override2
        {
            add
            {
                Console.WriteLine("Override2");
            }
            remove
            {
                Console.WriteLine("Override2");
            }
        }
    }
}
