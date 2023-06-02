
using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.ExplicitInterfaceMember;

[Inheritable]
public sealed class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        _ = meta.Target.Type.Methods.Where(x => x.Name.Contains("Bar")).First().Invoke();
        meta.Target.Type.Methods.Where(x => x.Name.Contains("Bar")).First().Invoke();
        _ = meta.Target.Type.Methods.Where(x => x.Name.Contains("Bar")).Skip(1).First().WithTypeArguments(typeof(int)).Invoke();
        meta.Target.Type.Methods.Where(x => x.Name.Contains("Bar")).Skip(1).First().WithTypeArguments(typeof(int)).Invoke();
        return meta.Proceed();
    }
}

public interface ITestInterface
{
    int Bar();

    int Bar<T>();
}

// <target>
public partial class TestClass : ITestInterface
{
    int ITestInterface.Bar()
    {
        return 42;
    }
    int ITestInterface.Bar<T>()
    {
        return 42;
    }

    [TestAspect]
    public void Foo()
    {
    }
}

