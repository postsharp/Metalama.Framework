﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Inliners.MethodLocalDeclaration_NotEqualsValueClause
{
    // <target>
    class Target
    {
        int Foo()
        {
            Console.WriteLine( "Original");
            return 42;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override()
        {
            Console.WriteLine( "Before");
            var (x, y) = (0, link( _this.Foo, inline)());
            Console.WriteLine( "After");
            return x;
        }
    }
}
