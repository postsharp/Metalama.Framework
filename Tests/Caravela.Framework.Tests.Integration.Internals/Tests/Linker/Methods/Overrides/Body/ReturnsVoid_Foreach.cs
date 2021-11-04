using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsVoid_Foreach
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
            foreach (var i in new[] { 1, 2, 3, 4, 5 })
            {
                link( _this.Foo, inline)();
            }

            Console.WriteLine( "After");
        }
    }
}
