using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.InvalidCode.DuplicateDeclaration;

/*
 * Tests that ambiguous declaration does not cause a crash in the linker. The output may not be correct.
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
    int Method(int a)
    {
        return a;
    }

#if TESTRUNNER
    int Method(int a)
    {
        return a;
    }
#endif
}