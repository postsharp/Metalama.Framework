using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable_CrossAssembly2;

[RunTimeOrCompileTime]
public class MiddleType : BaseType
{
    public int MiddleValue { get; }

    public MiddleType(int baseValue, int middleValue) : base(baseValue)
    {
        this.MiddleValue = middleValue;
    }
}