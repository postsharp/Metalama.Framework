﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable_CrossAssembly;

/*
 * The serializable base class of a serializable type defined in another assembly.
 */

[RunTimeOrCompileTime]
public class MiddleType : BaseType
{
    public int MiddleValue { get; }

    public MiddleType(int baseValue, int middleValue) : base(baseValue)
    {
        this.MiddleValue = middleValue;
    }
}

[RunTimeOrCompileTime]
public class DerivedType : MiddleType
{
    public int Value { get; }

    public DerivedType(int baseValue, int middleValue, int value) : base(baseValue, middleValue)
    {
        Value = value;
    }
}

public class TestAspect : OverrideMethodAspect
{
    public DerivedType SerializedValue;

    public TestAspect(int x, int y, int z)
    {
        SerializedValue = new DerivedType(x, y, z);
    }

    public override dynamic OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.BaseValue));
        Console.WriteLine(meta.CompileTime(SerializedValue.MiddleValue));
        Console.WriteLine(meta.CompileTime(SerializedValue.Value));
        return meta.Proceed();
    }

}

//<target>
public class TargetClass
{
    [TestAspect(13,27,42)]
    public void Foo()
    {
        Console.WriteLine("Original");
    }
}