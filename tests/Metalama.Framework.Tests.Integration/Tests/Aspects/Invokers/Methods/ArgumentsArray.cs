using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.ArgumentsArray;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.Target.Method.InvokeWithArgumentsObject(new[] { 1, 2 });
        return null;
    }
}

// <target>
internal class TargetClass
{
    [Test]
    void M(int i, int j) => Console.WriteLine(i + j);
}