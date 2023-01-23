using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Overrides.Inlining.ExpressionBody_NI
{
    // <target>
    class Target
    {
        [PseudoNotInlineable]
        int Foo => 42;

        [PseudoOverride(nameof(Foo),"TestAspect1")]
        int Foo_Override1
        {
            get
            {
                Console.WriteLine("Before1");
                var x = link(_this.Foo.get);
                Console.WriteLine("After1");
                return x;
            }
        }
    }
}
