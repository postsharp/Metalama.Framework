using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Signature.ReturnsVoid_IntParameter
{
    // <target>
    class Target
    {
        void Foo(int x)
        {
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override(int x)
        {
            link( _this.Foo, inline)(x);
        }
    }
}
