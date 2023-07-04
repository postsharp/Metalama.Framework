using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.ArgumentsArray;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.Target.Method.Invoke(1, 2);
        meta.Target.Method.InvokeWithArgumentsObject(new object[] { 1, 2 });
        return null;
    }
}

// <target>
internal class TargetClass
{
    [Test]
    void M(int i, int j) => Console.WriteLine(i + j);

    [Test]
    void M(int i, params int[] a) => Console.WriteLine(a[0] + a[1]);

    [Test]
    void M(params int[] a) => Console.WriteLine(a[0] + a[1]);
}