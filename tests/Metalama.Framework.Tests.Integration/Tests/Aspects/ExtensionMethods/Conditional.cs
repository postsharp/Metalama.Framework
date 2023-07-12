using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.Conditional;

#pragma warning disable CS0618 // Type or member is obsolete

internal class ReturnNumbers : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var numbers = new object[] { 42 };

        switch (meta.RunTime(DateTime.Today.DayOfWeek))
        {
            case DayOfWeek.Monday:
                return numbers?.ToList();
            case DayOfWeek.Tuesday:
                return numbers?.ToList().ToList();
            case DayOfWeek.Wednesday:
                return numbers.ToList()?.ToList();
            default:
                return numbers?.ToList()?.ToList();
        }
    }
}

internal class TargetCode
{
    // <target>
    [ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}