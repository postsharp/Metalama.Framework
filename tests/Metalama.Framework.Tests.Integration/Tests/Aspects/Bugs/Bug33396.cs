using System;
using System.IO;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33396;

public class TestAspect : MethodAspect
{
    private int _i;

    public TestAspect(int i)
    {
        this._i = i;
    }

    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        switch (this._i)
        {
            case 1:
                builder.Advice.Override(builder.Target, nameof(Template));
                break;
            case 2:
                builder.Advice.Override(builder.Target, nameof(Template), args: new { T = builder.Target.DeclaringType.DeclaringType });
                break;
            case 3:
                builder.Advice.Override(builder.Target, nameof(Template), args: new { T = "X" });
                break;
        }
    }

    [Template]
    public void Template<[CompileTime] T>()
    {
    }
}

// <target>
class Target
{
    [TestAspect(1)]
    public void M1()
    {
    }

    [TestAspect(2)]
    public void M2()
    {
    }

    [TestAspect(3)]
    public void M3()
    {
    }
}