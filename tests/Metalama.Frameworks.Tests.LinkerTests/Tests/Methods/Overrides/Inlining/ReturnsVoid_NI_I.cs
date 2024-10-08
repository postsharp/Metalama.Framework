using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Inlining.ReturnsVoid_NI_I
{
    // <target>
    class Target
    {

        void Foo()
        {
            Console.WriteLine( "Original");
        }

        [PseudoNotInlineable]
        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override()
        {
            Console.WriteLine( "Before");
            link( _this.Foo, inline)();
            Console.WriteLine( "After");
        }
    }
}
