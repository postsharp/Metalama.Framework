using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Signature.ReturnsVoid_NoParameter
{
    // <target>
    class Target
    {
        void Foo()
        {
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override()
        {
            link( _this.Foo, inline)();
        }
    }
}
