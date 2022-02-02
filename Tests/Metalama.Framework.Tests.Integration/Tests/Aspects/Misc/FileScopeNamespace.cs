using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.FileScopeNamespace;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Overridden.");
        return meta.Proceed();
    }
}

class TargetCode
{
    [Aspect]
    int Method(int a)
    {
        return a;
    }
}
