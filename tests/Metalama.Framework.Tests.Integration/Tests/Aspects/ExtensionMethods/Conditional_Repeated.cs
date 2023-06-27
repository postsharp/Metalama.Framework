using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.Conditional_Repeated;

#pragma warning disable CS0618 // Type or member is obsolete

internal class ReturnNumbers : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        IEnumerable<object>? numbers = new object[] { 42 };

        foreach (var _ in meta.CompileTime(Enumerable.Range(1, 2)))
        {
            numbers = numbers?.ToList();
        }

        return numbers;
    }
}

internal class TargetCode
{
    // <target>
    [ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}