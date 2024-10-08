using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TargetBody.ReturnsInt_ExpressionBody
{
    // <target>
    class Target
    {
        int Foo() => 42;

        [PseudoOverride( nameof(Foo), "TestAspect")]

        int Foo_Override()
        {
            Console.WriteLine("Aspect");
            return link(_this.Foo, inline)();
        }
    }
}
