using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ValueType_InterfaceImpl;

/*
 * A serializable value type explicitly implementing an interface.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}