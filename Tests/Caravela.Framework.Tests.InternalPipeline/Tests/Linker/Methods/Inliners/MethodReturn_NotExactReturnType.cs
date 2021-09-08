﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodReturn_NotExactReturnType
{
    // <target>
    class Target
    {
        int Foo()
        {
            Console.WriteLine( "Original");
            return 42;
        }

        short Foo2()
        {
            Console.WriteLine("Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override()
        {
            Console.WriteLine( "Before");
            return link( _this.Foo2, inline)();
        }
    }
}
