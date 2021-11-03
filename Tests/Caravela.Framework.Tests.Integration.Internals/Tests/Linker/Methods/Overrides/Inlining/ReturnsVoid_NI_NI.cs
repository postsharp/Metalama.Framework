using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Inlining.ReturnsVoid_NI_NI
{
    // <target>
    class Target
    {
        void Foo()
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        [PseudoNotInlineable]
        void Foo_Override()
        {
            Console.WriteLine( "Before");
            link( _this.Foo)();
            Console.WriteLine( "After");
        }
    }
}
