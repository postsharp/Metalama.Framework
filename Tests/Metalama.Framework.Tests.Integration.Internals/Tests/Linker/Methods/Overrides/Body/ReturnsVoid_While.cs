using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsVoid_While
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
            int i = 0;
            while (i < 5)
            {
                link( _this.Foo, inline)();
                i++;
            }

            Console.WriteLine( "After");
        }
    }
}
