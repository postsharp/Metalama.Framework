﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Properties.Inliners.GetterCastReturn
{
    // <target>
    class Target
    {
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
                return (int)link( _this.Foo.get, inline);
            }
        }
    }
}
