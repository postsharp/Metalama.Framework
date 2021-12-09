using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodInvocation_NotExactArguments
{
    // <target>
    class Target
    {
        void Foo(int x, int y)
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override(int x, int y)
        {
            Console.WriteLine( "Before");
            link( _this.Foo, inline)(y, x);
            Console.WriteLine( "After");
        }
    }
}
