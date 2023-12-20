using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.GetOnlyAutopropertyOverride;

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