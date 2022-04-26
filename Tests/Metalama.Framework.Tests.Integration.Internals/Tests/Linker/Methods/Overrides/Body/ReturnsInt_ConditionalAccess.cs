﻿#if TEST_OPTIONS
// @Skipped(Linker test preprocessing does not correctly support conditional access expressions)
#endif

using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.Body.ReturnsInt_ConditionalAccess
{
    // <target>
    internal class Target
    {
        private int Foo( Target? x )
        {
            Console.WriteLine( "Original" );

            return 42;
        }

        [PseudoOverride( nameof(Foo), "TestAspect" )]
        private int? Foo_Override( Target? x )
        {
            Console.WriteLine( "Before" );
            int? result = null;
            result = _local.x?.link( _local.Foo, inline )( this );

            Console.WriteLine( "After" );

            return result;
        }
    }
}