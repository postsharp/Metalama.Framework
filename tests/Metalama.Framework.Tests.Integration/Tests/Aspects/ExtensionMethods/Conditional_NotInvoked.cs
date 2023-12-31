#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.Conditional_NotInvoked;

#pragma warning disable CS0618 // Type or member is obsolete

internal class ReturnNumbers : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
#if TESTRUNNER
        var numbers = new object[] { 42 };

        return numbers?.ToHashSet;
#else
        return null;
#endif
    }
}

internal class TargetCode
{
    // <target>
    [ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}