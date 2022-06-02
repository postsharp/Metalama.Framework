﻿#if TEST_OPTIONS
// @ExpectedEndOfLine(CRLF)
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.EndOfLines_CRLF;

[assembly: AspectOrder( typeof(TestAspect1), typeof(TestAspect2) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.EndOfLines_CRLF
{
    public class TestAspect1 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            meta.InsertStatement( "Console.WriteLine(\"Hello!\");\n" );

            return meta.Proceed();
        }
    }

    public class TestAspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            meta.InsertStatement( "Console.WriteLine(\"Hello!\");\r" );

            return meta.Proceed();
        }
    }

    // <target>
    public class Target
    {
        [TestAspect1]
        [TestAspect2]
        private static int Add( int a, int b )
        {
            Console.WriteLine( "Thinking..." );

            return a + b;
        }
    }
}