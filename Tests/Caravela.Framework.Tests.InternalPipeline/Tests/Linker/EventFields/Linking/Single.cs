using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Linking.Single
{
    // <target>
    class Target
    {
        delegate void Handler();
        void Test(string s)
        {
        }

        event Handler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect")]
        event Handler? Foo_Override
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
