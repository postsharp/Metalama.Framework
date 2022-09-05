using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TemplateBody.ReturnsVoid_While
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
