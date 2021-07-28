using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsVoid_For
{
    // <target>
    class Target
    {
        void Foo()
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override()
        {
            Console.WriteLine( "Before");
            for (int i = 0; i < 5; i++)
            {
                link( _this.Foo, inline)();
            }

            Console.WriteLine( "After");
        }
    }
}
