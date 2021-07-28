using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Inliners.AddAssignment
{
    // <target>
    public class Target
    {
        private EventHandler? field;

        event EventHandler Foo
        {
            add
            {
                Console.WriteLine("Original");
                this.field += value;
            }
            remove { }
        }

        [PseudoOverride(nameof(Foo), "TestAspect")]
        event EventHandler Foo_Override
        {
            add
            {
                Console.WriteLine("Before");
                link[_this.Foo.add, inline] += value;
                Console.WriteLine("After");
            }
            remove { }
        }
    }
}
