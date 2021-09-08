using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotAssignment
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
                link[_this.Foo.add, inline](null, null);
                Console.WriteLine("After");
            }
            remove { }
        }
    }
}
