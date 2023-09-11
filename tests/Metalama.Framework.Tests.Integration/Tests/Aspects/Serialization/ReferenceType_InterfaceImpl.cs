using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ReferenceType_InterfaceImpl;

/*
 * A serializable reference type implicitly implementing an interface.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}