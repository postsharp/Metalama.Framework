using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.Conditional_Statement;

#pragma warning disable CS0618 // Type or member is obsolete

internal class ReturnNumbers : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var numbers = new object[] { 42 };

        numbers?.ToList()?.ToList();
        numbers!.ToList()?.ToList();
        numbers?.ToList().ToList();

        return null;
    }
}

internal class TargetCode
{
    // <target>
    [ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}