using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32493;

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}