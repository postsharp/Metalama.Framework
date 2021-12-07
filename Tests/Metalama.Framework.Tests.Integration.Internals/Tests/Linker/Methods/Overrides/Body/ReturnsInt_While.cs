﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsInt_While
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
            int i = 0;
            int k = 0;
            while (i < 0)
            {
                int result;
                result = link( _this.Foo, inline)(x);
                k += result;
                i++;
            }

            Console.WriteLine( "After");
            return k;
        }
    }
}
