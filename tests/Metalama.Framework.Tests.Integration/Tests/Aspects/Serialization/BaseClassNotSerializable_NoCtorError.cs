using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassNotSerializable_NoCtorError;

/*
 * The base class of a serializable typeis not itself serializable and does not have a parameterless base constructor.
 */

[RunTimeOrCompileTime]
public class BaseType
{
    public int BaseValue { get; }

    public BaseType(int baseValue)
    {
        this.BaseValue = 13;
    }
}

[RunTimeOrCompileTime]
public class DerivedType : BaseType, ICompileTimeSerializable
{
    public int Value { get; }

    public DerivedType(int value, int baseValue) : base(baseValue)
    {
        Value = value;
    }
}