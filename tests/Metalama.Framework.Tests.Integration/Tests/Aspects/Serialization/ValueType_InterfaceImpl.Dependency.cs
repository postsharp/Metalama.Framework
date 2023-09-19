﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.ValueType_InterfaceImpl;

[RunTimeOrCompileTime]
public struct ValueType : ICompileTimeSerializable
{
    public int Value { get; set; }

    public int ValueGetOnly { get; }

    public int ValueInitOnly { get; init; }

    public ValueType(int value)
    {
        this.Value = value;
        this.ValueGetOnly = value;
        this.ValueInitOnly = value;
    }
}

[Inheritable]
public class TestAspect : OverrideMethodAspect
{
    public ValueType SerializedValue;

    public TestAspect(int x)
    {
        SerializedValue = new ValueType(x);
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.CompileTime(SerializedValue.Value));
        Console.WriteLine(meta.CompileTime(SerializedValue.ValueGetOnly));
        Console.WriteLine(meta.CompileTime(SerializedValue.ValueInitOnly));
        return meta.Proceed();
    }
}

public class BaseClass
{
    [TestAspect(42)]
    public virtual void Foo()
    {
        Console.WriteLine("Original");
    }
}