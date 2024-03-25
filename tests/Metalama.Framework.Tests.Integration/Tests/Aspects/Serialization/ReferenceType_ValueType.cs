using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ReferenceType_ValueType;

/*
 * The serializable reference type.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}