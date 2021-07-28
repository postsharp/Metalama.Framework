﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Jump.ReturnsVoid_FJ_NJ_FJ
{
    // <target>
    class Target
    {
        void Foo(int x)
        {
            Console.WriteLine( "Original Start");
            if (x == 0)
            {
                return;
            }

            Console.WriteLine( "Original End");
        }

        [PseudoOverride( nameof(Foo),"TestAspect1")]
        void Foo_Override1(int x)
        {
            Console.WriteLine( "Before1");
            link( _this.Foo, inline)(x);
            Console.WriteLine( "After1");
        }

        [PseudoOverride( nameof(Foo),"TestAspect2")]
        void Foo_Override2(int x)
        {
            Console.WriteLine( "Before2");
            if (x == 0)
            {
                return;
            }

            link( _this.Foo, inline)(x);
            Console.WriteLine( "After2");
        }

        [PseudoOverride( nameof(Foo),"TestAspect3")]
        void Foo_Override3(int x)
        {
            Console.WriteLine( "Before3");
            link( _this.Foo, inline)(x);
            Console.WriteLine( "After3");
        }
    }
}
