using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TemplateBody.ReturnsVoid_For
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
