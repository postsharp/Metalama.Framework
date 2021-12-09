using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodDiscard_NotExactArguments
{
    // <target>
    class Target
    {
        int Foo(int x, int y)
        {
            Console.WriteLine( "Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override(int x, int y)
        {
            Console.WriteLine( "Before");
            _ = link( _this.Foo, inline)(y, x);
            Console.WriteLine( "After");
            return 42;
        }
    }
}
