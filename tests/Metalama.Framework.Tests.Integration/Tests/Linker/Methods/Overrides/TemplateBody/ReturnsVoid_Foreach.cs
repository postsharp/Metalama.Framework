﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TemplateBody.ReturnsVoid_Foreach
{
    // <target>
    class Target
    {
        void Foo()
        {
            Console.WriteLine( "Original");
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        void Foo_Override()
        {
            Console.WriteLine( "Before");
            foreach (var i in new[] { 1, 2, 3, 4, 5 })
            {
                link( _this.Foo, inline)();
            }

            Console.WriteLine( "After");
        }
    }
}
