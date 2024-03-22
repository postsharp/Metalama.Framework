using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Signature.ReturnsInt_IntParameter
{
    // <target>
    class Target
    {
        int Foo(int x)
        {
            return x;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override(int x)
        {
            return link( _this.Foo, inline)(x);
        }
    }
}
