#if TEST_OPTIONS
// @SkipAddingSystemFiles
// @RequiredConstant(NETFRAMEWORK)
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.GetOnlyAutopropertyOverride_NetFramework;

public sealed class TestAspect : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get
        {
            Console.WriteLine("getter");

            return meta.Proceed();
        }
        set
        {
            Console.WriteLine("setter");

            meta.Proceed();
        }
    }
}

// <target>
public class Target
{
    [TestAspect]
    public int Test { get; }
}