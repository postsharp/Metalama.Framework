using System;
using Metalama.Framework.Aspects;
#if TEST_OPTIONS
// @TestScenario(Preview)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Preview.BasicTest;

internal class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Oops" );

        return meta.Proceed();
    }
}

internal class C
{
    [TheAspect]
    public void M() { }
}