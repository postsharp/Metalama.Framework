﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TargetBody.SwitchInt_EndFlow
{
    // <target>
    class Target
    {
        int Foo(int x)
        {
            switch(x)
            {
                case 1:
                    return 42;
                default:
                    return 0;
            }
        }

        [PseudoOverride( nameof(Foo), "TestAspect")]

        int Foo_Override(int x)
        {
            Console.WriteLine("Aspect");
            var z = link(_this.Foo, inline)(x);
            return z;
        }
    }
}
