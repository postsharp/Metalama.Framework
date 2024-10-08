﻿using System;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Proceed.ReturnsVoid_NP_NP
{
    // <target>
    class Target
    {
        void Foo(int x)
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect1")]
        void Foo_Override1(int x)
        {
            Console.WriteLine( "Override1");
        }

        [PseudoOverride( nameof(Foo),"TestAspect2")]
        void Foo_Override2(int x)
        {
            Console.WriteLine( "Override2");
        }
    }
}
