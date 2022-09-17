using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TargetBody.ReturnsVoid_ExpressionBody
{
    // <target>
    class Target
    {
        void Foo() => Console.WriteLine( "Original");

        [PseudoOverride( nameof(Foo), "TestAspect")]

        void Foo_Override()
        {
            Console.WriteLine("Aspect");
            link(_this.Foo, inline)();
        }
    }
}
