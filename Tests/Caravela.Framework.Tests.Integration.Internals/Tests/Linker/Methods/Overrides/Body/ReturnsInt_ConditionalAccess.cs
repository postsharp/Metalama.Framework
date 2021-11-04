// @Skipped(Linker test preprocessing does not correctly support conditional access expressions)

using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsInt_ConditionalAccess
{
    // <target>
    class Target
    {
        int Foo(Target? x)
        {
            Console.WriteLine( "Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int? Foo_Override(Target? x)
        {
            Console.WriteLine( "Before");
            int? result = null;
            result = _local.x?.link(_local.Foo, inline)(this);

            Console.WriteLine( "After");
            return result;
        }
    }
}
