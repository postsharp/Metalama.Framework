#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if ROSLYN_4_4_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.LiteralSuffixes_Utf8;

public class TestAspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var s1 = "littoral literal"u8;
        ReadOnlySpan<byte> s2 = s1;        

        return meta.Proceed();
    }
}

public class TargetClass
{
    // <target>
    [TestAspect]
    public void Method()
    {
    }
}

#endif