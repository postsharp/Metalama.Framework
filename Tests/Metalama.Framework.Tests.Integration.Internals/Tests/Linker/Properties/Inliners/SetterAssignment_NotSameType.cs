﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Inliners.SetterAssignment_NotSameType
{
    class Base
    {
        public virtual int Foo
        {
            set
            {
            }
        }
    }

    // <target>
    class Target : Base
    {
        [PseudoIntroduction(nameof(Foo), "TestAspect")]
        public override int Foo
        {
            set
            {
                Console.WriteLine( "Original");
            }
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override
        {
            set
            {
                Console.WriteLine( "Before");
                link[ _this.Foo.set, inline, @base] = value;
                Console.WriteLine( "After");
            }
        }
    }
}
