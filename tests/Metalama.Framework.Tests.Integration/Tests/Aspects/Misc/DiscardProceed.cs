using System;
using System.Collections.Generic;
using System.IO;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.DiscardProceed;

public class TestAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.Proceed();
        return default;
    }
}

// <target>
class Target
{
    [Test]
    MemoryStream M1()
    {
        return new();
    }

    [Test]
    MemoryStream M2() => new();
}