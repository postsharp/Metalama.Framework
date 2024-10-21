﻿using System;
using static Metalama.Framework.Tests.LinkerTests.Tests.Api;

namespace Metalama.Framework.Tests.LinkerTests.Tests.Properties.Inliners.SetterAssignment
{
    // <target>
    class Target
    {
        int field;

        int Foo
        {
            set
            {
                Console.WriteLine( "Original");
                this.field = value;
            }
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override
        {
            set
            {
                Console.WriteLine( "Before");
                link[ _this.Foo.set, inline ] = value;
                Console.WriteLine( "After");
            }
        }
    }
}