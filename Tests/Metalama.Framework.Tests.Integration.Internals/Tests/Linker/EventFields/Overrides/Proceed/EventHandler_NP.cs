using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Proceed.EventHandler_NP
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect")]
        event EventHandler Foo_Override1
        {
            add
            {
                Console.WriteLine("Override");
            }
            remove
            {
                Console.WriteLine("Override");
            }
        }
    }
}
