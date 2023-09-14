using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.Conditional_CompileTime;

#pragma warning disable CS0618 // Type or member is obsolete

internal class ReturnNumbers : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var numbers = meta.CompileTime(new int[] { 42 });

        switch (DateTime.Today.DayOfWeek)
        {
            case DayOfWeek.Monday:
                return numbers?.ToReadOnlyList();
            case DayOfWeek.Tuesday:
                return numbers?.ToReadOnlyList().ToReadOnlyList();
            case DayOfWeek.Wednesday:
                return numbers.ToReadOnlyList()?.ToReadOnlyList();
            default:
                return numbers?.ToReadOnlyList()?.ToReadOnlyList();
        }
    }
}

internal class TargetCode
{
    // <target>
    [ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}