using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Inlining.EventHandler_I_NI_I
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect1")]
        event EventHandler? Foo_Override1
        {
            add
            {
                Console.WriteLine("Before1");
                link[_this.Foo, inline] += value;
                Console.WriteLine("After1");
            }

            remove
            {
                Console.WriteLine("Before1");
                link[_this.Foo, inline] -= value;
                Console.WriteLine("After1");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        event EventHandler? Foo_Override2
        {
            add
            {
                Console.WriteLine( "Before2");
                link[_this.Foo] += value;
                Console.WriteLine( "After2");
            }

            remove
            {
                Console.WriteLine("Before2");
                link[_this.Foo] -= value;
                Console.WriteLine("After2");
            }
        }
    }
}
