using System;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Proceed.ReturnsVoid_NP
{
    // <target>
    class Target
    {
        void Foo(int x)
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override(int x)
        {
            Console.WriteLine( "Override");
        }
    }
}
