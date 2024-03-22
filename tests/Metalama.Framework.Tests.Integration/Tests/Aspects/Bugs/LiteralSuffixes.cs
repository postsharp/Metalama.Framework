using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.LiteralSuffixes;

public class TestAspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var f1 = 0.5f;
        float f2 = f1;

        var d1 = 0.5d;
        double d2 = d1;

        var m1 = 0.5m;
        decimal m2 = m1;

        var u1 = 0u;
        uint u2 = u1;

        var l1 = 0L;
        long l2 = l1;

        var ul1 = 0UL;
        ulong ul2 = ul1;

        var x1 = 0x12345678ABCDEFul;
        ulong x2 = x1;

        var b1 = 0b01_10;
        int b2 = b1;

        return meta.Proceed();
    }
}

public class TargetClass
{
    // <target>
    [TestAspect]
    public void Method()
    {
    }
}