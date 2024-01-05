using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.InvalidCode.Override_MissingType;

/*
 * Tests that call of a method of a missing type is handled by overrides.
 */

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Aspect");
        return meta.Proceed();
    }

}

// <target>
class TargetCode
{
    [Aspect]
    void Method(int a)
    {
#if TESTRUNNER
        Foo.Bar();
#endif
    }

}