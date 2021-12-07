﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodInvocation_NotSameType
{
    class Base
    {
        public virtual int Foo()
        {
            return 0;
        }
    }

    // <target>
    class Target : Base
    {
        [PseudoIntroduction(nameof(Foo), "TestAspect")]
        public override int Foo()
        {
            Console.WriteLine( "Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override()
        {
            Console.WriteLine( "Before");
            link( _this.Foo, inline, @base)();
            Console.WriteLine( "After");
            return 42;
        }
    }
}
