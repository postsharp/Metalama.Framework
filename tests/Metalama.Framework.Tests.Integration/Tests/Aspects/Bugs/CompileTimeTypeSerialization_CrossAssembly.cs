using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.CompileTimeTypeSerialization_CrossAssembly;

// <target>
public sealed class TestClass : BaseClass, ICloneable
{
    public object Clone() => throw new NotImplementedException();
}