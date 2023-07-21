using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33532;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();

        throw new NotImplementedException();
    }
}

// <target>
class Target
{
    [Aspect]
    static void UnreachableAfterReturn() 
    { 
        return; 
        throw new Exception();
    }

    [Aspect]
    static void ReachableAfterReturn(int i)
    {
        if (i == 0)
        {
            goto label;
        }
        return;

        label:
        Console.WriteLine("Test");
    }
}