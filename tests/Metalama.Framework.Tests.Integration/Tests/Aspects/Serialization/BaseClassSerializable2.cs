using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable2;

/*
 * The serializable base class of a serializable type.
 */

[RunTimeOrCompileTime]
public class BaseType : ICompileTimeSerializable
{
    public int BaseValue { get; }

    public BaseType(int baseValue)
    {
        this.BaseValue = baseValue;
    }
}

[RunTimeOrCompileTime]
public class DerivedType : BaseType
{
    public int Value { get; }

    public DerivedType(int baseValue, int value) : base(baseValue)
    {
        Value = value;
    }
}

public class TestAspect : OverrideMethodAspect
{
    public DerivedType SerializedValue;

    public TestAspect(int x, int y)
    {
        SerializedValue = new DerivedType(x, y);
    }

    public override dynamic OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.BaseValue));
        Console.WriteLine(meta.CompileTime(SerializedValue.Value));
        return meta.Proceed();
    }

}

//<target>
public class TargetClass
{
    [TestAspect(13,42)]
    public void Foo()
    {
        Console.WriteLine("Original");
    }
}