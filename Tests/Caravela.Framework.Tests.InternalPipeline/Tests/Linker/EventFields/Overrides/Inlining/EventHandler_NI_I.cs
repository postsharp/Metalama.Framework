﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Inlining.EventHandler_NI_I
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoNotInlineable]
        [PseudoOverride( nameof(Foo),"TestAspect")]
        event EventHandler? Foo_Override
        {
            add
            {
                Console.WriteLine("Before");
                link[_this.Foo, inline] += value;
                Console.WriteLine("After");
            }

            remove
            {
                Console.WriteLine("Before");
                link[_this.Foo, inline] -= value;
                Console.WriteLine("After");
            }
        }
    }
}
