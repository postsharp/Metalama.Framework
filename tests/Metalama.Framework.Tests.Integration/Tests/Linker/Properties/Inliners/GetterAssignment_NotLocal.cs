﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Inliners.GetterAssignment_NotLocal
{
    // <target>
    class Target
    {
        int x;

        int Foo
        {
            get
            {
                Console.WriteLine( "Original");
                return 42;
            }
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override
        {
            get
            {
                Console.WriteLine( "Before");
                x = link( _this.Foo.get, inline );
                Console.WriteLine( "After");
                return x;
            }
        }
    }
}
