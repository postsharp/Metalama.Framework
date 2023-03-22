using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassNotSerializable_CrossAssembly;

[RunTimeOrCompileTime]
public class BaseType
{
    public int BaseValue { get; }

    public BaseType()
    {
        this.BaseValue = 13;
    }
}
