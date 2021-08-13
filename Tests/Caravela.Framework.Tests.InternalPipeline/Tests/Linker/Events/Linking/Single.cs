using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Linking.Single
{
    // <target>
    class Target
    {        
        EventHandler? field;

        event EventHandler? Foo
        {
            add
            {
                this.field += value;
            }
            remove
            {
                this.field -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect")]
        event EventHandler? Foo_Override
        {
            add
            {
                link[_this.Foo.add] += value;
            }
            remove
            {
                link[_this.Foo.remove] -= value;
            }
        }
    }
}
