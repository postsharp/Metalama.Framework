using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Inliners.RemoveAssignment
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect")]
        event EventHandler? Foo_Override
        {
            add { }
            remove
            {
                Console.WriteLine("Before");
                link[_this.Foo.remove, inline] -= value;
                Console.WriteLine("After");
            }
        }
    }
}
