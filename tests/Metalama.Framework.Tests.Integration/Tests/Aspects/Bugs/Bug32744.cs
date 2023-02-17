namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32744;

using Metalama.Framework.Aspects;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {        
        return meta.Proceed();

        void Foo(int i) {}
    }
}

internal class C
{
 

    // <target>
    [Test]
    private static int Bar()
    {
        return 42;
    }
}