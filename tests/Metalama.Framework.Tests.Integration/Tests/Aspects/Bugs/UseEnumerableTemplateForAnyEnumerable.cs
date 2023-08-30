using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.UseEnumerableTemplateForAnyEnumerable;

public class AspectAttribute : OverrideMethodAspect
{
    public AspectAttribute()
    {
        this.UseEnumerableTemplateForAnyEnumerable = true;
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("default");

        return meta.Proceed();
    }

    public override IEnumerable<dynamic?> OverrideEnumerableMethod()
    {
        Console.WriteLine("enumerable");

        return meta.ProceedEnumerable();
    }
}

// <target>
class EmptyOverrideFieldOrPropertyExample
{
    [Aspect]
    IEnumerable<int> M()
    {
        return new[] { 42 };
    }
}