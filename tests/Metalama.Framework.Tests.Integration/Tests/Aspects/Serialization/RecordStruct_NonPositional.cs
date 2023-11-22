using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.RecordStruct_NonPositional;

/*
 * The non-positional record struct.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}