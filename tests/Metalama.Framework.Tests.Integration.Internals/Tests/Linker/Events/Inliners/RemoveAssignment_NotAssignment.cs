using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotAssignment
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
                link[_this.Foo.add, inline]?.Invoke(null, null);
                Console.WriteLine("After");
            }
        }
    }
}
