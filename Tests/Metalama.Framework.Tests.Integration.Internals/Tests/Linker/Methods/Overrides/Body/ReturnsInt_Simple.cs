﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsInt_Simple
{
    // <target>
    class Target
    {
        int Foo(int x)
        {
            Console.WriteLine( "Original");
            return x;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override(int x)
        {
            Console.WriteLine( "Before");
            int result;
            result = link( _this.Foo, inline)(x);
            Console.WriteLine( "After");
            return result;
        }
    }
}
