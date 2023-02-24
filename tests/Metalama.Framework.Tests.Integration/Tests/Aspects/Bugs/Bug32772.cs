﻿#if TEST_OPTIONS
// @RemoveAssemblyAttributes(true)
# endif 

using Metalama.Framework.Aspects;
using System.Reflection;

#if TESTRUNNER
[assembly: AssemblyVersion("0.0.*")]
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs;

/* 
 * Tests that having a wildcarded assembly version in a (deterministic) compilation does pass through the pipeline.
 * This tests the case when the version would not change. There are standalone tests for the situation where the version would change.
 * 
 * NOTE: Deterministic compilations do not allow this, but the diagnostic is produced on the initial compilation and is removed by
 *       fixing the version in the intermediate compilation.
 */

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