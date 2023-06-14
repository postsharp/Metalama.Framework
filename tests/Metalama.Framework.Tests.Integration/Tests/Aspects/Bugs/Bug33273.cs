
using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33273;

[Inheritable]
public sealed class TestAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        _ = meta.Cast(meta.Target.Method.ReturnType, StaticClass.StaticMethod());

        return meta.Proceed();
    }
}

// <target>
public partial class TargetClass
{
    [TestAspect]
    public int Foo()
    {
        return 42;
    }
}

public class StaticClass
{
    public static double StaticMethod() => Math.PI;
}

