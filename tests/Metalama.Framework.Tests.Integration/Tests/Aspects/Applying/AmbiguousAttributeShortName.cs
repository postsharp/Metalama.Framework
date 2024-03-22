using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Applying.AmbiguousAttributeShortName;

public class RequiresAttributeAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Applied.");

        return null;
    }
}

// <target>
class TargetClass
{
    [RequiresAttribute]
    void ShortName() { }

    [RequiresAttributeAttribute]
    void LongName() { }
}