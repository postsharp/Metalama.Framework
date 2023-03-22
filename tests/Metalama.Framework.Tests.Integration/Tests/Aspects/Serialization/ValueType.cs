using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ValueType;

/*
 * The serializable value type.
 */

[RunTimeOrCompileTime]
public struct ValueType : ICompileTimeSerializable
{
    public int Value { get; }

    public ValueType(int value)
    {
        this.Value = value;
    }
}

public class TestAspect : OverrideMethodAspect
{
    public ValueType SerializedValue;

    public TestAspect(int x)
    {
        SerializedValue = new ValueType(x);
    }

    public override dynamic OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.Value));
        return meta.Proceed();
    }

}

//<target>
public class TargetClass
{
    [TestAspect(42)]
    public void Foo()
    {
        Console.WriteLine("Original");
    }
}