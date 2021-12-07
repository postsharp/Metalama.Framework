﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodAssignment_NotLocal
{
    // <target>
    class Target
    {
        int x;

        int Foo()
        {
            Console.WriteLine( "Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override()
        {
            Console.WriteLine( "Before");
            x = link( _this.Foo, inline)();
            Console.WriteLine( "After");
            return x;
        }
    }
}
