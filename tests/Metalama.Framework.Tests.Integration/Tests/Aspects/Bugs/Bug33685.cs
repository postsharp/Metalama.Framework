using System;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33685;

public class TestAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template));
    }

    [Template]
    public dynamic? Template<T>()
    {
        var method = meta.Target.Type.Methods.OfName("Bar").Single();
        method.Invoke(new TestData<T>());
        return meta.Proceed();
    }
}

[RunTimeOrCompileTime]
public class TestData<T>
{
}

// <target>
class Target
{
    [TestAspect]
    public void Foo<T>()
    {
    }

    public void Bar<T>(TestData<T> data)
    {
    }

}