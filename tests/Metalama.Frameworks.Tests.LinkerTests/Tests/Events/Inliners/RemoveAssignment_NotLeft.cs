using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067, CS0649

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotLeft
{
    // <target>
    public class Target
    {
        event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect")]
        private event EventHandler Foo_Override
        {
            add
            {
            }
            remove
            {
                Console.WriteLine("Before");
                EventHandler? x = null;
                x -= link[_this.Foo.add, inline];
                Console.WriteLine("After");
            }
        }
    }
}
