using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ReferenceType_ManualBase;

/*
 * The base serializer is manually written.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}