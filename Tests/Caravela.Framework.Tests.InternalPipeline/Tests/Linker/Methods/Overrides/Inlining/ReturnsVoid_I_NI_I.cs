﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Inlining.ReturnsVoid_I_NI_I
{
    // <target>
    class Target
    {
        void Foo()
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect1")]
        void Foo_Override1()
        {
            Console.WriteLine( "Before1");
            link( _this.Foo, inline)();
            Console.WriteLine( "After1");
        }

        [PseudoOverride( nameof(Foo),"TestAspect2")]
        void Foo_Override2()
        {
            Console.WriteLine( "Before2");
            link( _this.Foo)();
            Console.WriteLine( "After2");
        }
    }
}
