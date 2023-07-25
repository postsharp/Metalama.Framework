using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ReferenceType;

[RunTimeOrCompileTime]
public class ReferenceType : ICompileTimeSerializable
{
    public int Value { get; }

    public ReferenceType(int value)
    {
        this.Value = value;
    }
}

[Inheritable]
public class TestAspect : OverrideMethodAspect
{
    public ReferenceType SerializedValue;

    public TestAspect(int x)
    {
        SerializedValue = new ReferenceType(x);
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.Value));
        return meta.Proceed();
    }

}

public class BaseClass
{
    [TestAspect(42)]
    public virtual void Foo()
    {
    }
}