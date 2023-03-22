using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable_CrossAssembly2;

[RunTimeOrCompileTime]
public class BaseType : ICompileTimeSerializable
{
    public int BaseValue { get; }

    public BaseType(int baseValue)
    {
        this.BaseValue = baseValue;
    }
}