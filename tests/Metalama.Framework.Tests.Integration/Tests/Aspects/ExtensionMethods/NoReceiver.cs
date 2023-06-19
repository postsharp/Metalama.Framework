using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.ExtensionMethods.NoReceiver;

#pragma warning disable CS0618 // Type or member is obsolete

[RunTimeOrCompileTime]
static class Outer
{
    internal static List<T> MyToList<T>(this IEnumerable<T> source) => source.ToList();

    internal class ReturnNumbers : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var numbers = new object[] { 42 };

            return numbers.MyToList();
        }
    }
}

internal class TargetCode
{
    // <target>
    [Outer.ReturnNumbers]
    private object Method() => throw new NotImplementedException();
}