
using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32906;

[Inheritable]
public sealed class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.Target.Method.DeclaringType.Methods.OfName("Foo").Single().With(InvokerOptions.Base).Invoke();
        return meta.Proceed();
    }
}

public class BaseClass
{
    public virtual void Foo() 
    { 
    }
}

// <target>
public partial class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Override method with no aspect override");
    }

    [TestAspect]
    public void Bar()
    {
    }
}

