using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects;

public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Aspect1" );

        return meta.Proceed();
    }
}

public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Aspect2" );

        return meta.Proceed();
    }
}


public class T
{
    [Aspect1, Aspect2]
    public void M() { }
}