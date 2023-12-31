﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.NoSerializableFields_CrossAssembly;

[RunTimeOrCompileTime]
public class BaseType : ICompileTimeSerializable
{
    public int BaseValue { get; }

    public ValueContainer BaseContainer { get; }

    public BaseType(int baseValue)
    {
        this.BaseValue = baseValue;
        this.BaseContainer = new ValueContainer(baseValue);
    }
}

[RunTimeOrCompileTime]
public class ValueContainer : ICompileTimeSerializable
{
    public int Value { get; }

    public ValueContainer(int value)
    {
        this.Value = value;
    }
}