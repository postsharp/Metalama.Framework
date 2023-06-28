
using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.ExplicitInterfaceMember;

[Inheritable]
public sealed class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.Target.Type.Events.Single().Add(null);
        meta.Target.Type.Events.Single().Remove(null);
        return meta.Proceed();
    }
}

public interface ITestInterface
{
    event EventHandler? Bar;
}

// <target>
public partial class TestClass : ITestInterface
{
    event EventHandler? ITestInterface.Bar
    {
        add { }
        remove { }
    }

    [TestAspect]
    public void Foo()
    {
    }
}

