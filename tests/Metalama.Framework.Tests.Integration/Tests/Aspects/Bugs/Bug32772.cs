#if TEST_OPTIONS
#endif

using Metalama.Framework.Aspects;
using System.Reflection;

#if TESTRUNNER
[assembly: AssemblyVersion("1.0.*")]
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();
    }
}

// <target>
internal class C
{
    [Test]
    private static int Bar()
    {
        return 42;
    }
}