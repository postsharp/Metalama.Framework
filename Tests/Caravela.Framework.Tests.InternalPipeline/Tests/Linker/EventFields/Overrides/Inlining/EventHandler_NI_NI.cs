using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Inlining.EventHandler_NI_NI
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride( nameof(Foo),"TestAspect")]
        [PseudoNotInlineable]
        event EventHandler? Foo_Override
        {
            add
            {
                Console.WriteLine("Before");
                link[_this.Foo] += value;
                Console.WriteLine("After");
            }

            remove
            {
                Console.WriteLine("Before");
                link[_this.Foo] -= value;
                Console.WriteLine("After");
            }
        }
    }
}
