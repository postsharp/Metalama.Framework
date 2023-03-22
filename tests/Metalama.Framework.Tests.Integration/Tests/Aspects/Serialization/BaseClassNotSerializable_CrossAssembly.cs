using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassNotSerializable_CrossAssembly;

/*
 * The base class of a serializable type defined in another assembly is not itself serializable and has a parameterless base constructor.
 */

[RunTimeOrCompileTime]
public class DerivedType : BaseType, ICompileTimeSerializable
{
    public int Value { get; }

    public DerivedType(int value)
    {
        Value = value;
    }
}

public class TestAspect : OverrideMethodAspect
{
    public DerivedType SerializedValue;

    public TestAspect(int z)
    {
        SerializedValue = new DerivedType(z);
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