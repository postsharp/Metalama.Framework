using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.InvalidCode.DuplicateDeclarationWithAspect;

/*
 * Tests that when there are duplicate declarations, the error is produced without crashing.
 */

public class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();
    }
}

class TargetCode
{
    [Aspect]
    public int Foo()
    {
        return 42;
    }

#if TESTRUNNER
    [Aspect]
    public int Foo()
    {
        return 42;
    }
#endif
}