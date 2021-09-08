using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotLeft
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
                Console.WriteLine("Before");
                EventHandler? x = null;
                x -= link[_this.Foo.add, inline];
                Console.WriteLine("After");
            }
            remove { }
        }
    }
}
