// TODO: This is a copy of _Redistribution.Dependency.cs file and it can be removed when the #30975 is fixed.
#define ASPECTS_IN_REDISTRIBUTION_NAMESPACE

using Metalama.Framework.Aspects;
using System;

// TODO: Uncomment when #30975 is fixed.

//#if ASPECTS_IN_REDISTRIBUTION_NAMESPACE
//using RedistributionTests.TargetNamespace;
//#else
//using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;
//#endif
//
//[assembly: AspectOrder(typeof(RedistributionAspect1), typeof(RedistributionAspect2), typeof(RedistributionAspect3), typeof(RedistributionAspect4))]

#if ASPECTS_IN_REDISTRIBUTION_NAMESPACE
namespace RedistributionTests.TargetNamespace;
#else
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;
#endif

public class RedistributionAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect1));
        return meta.Proceed();
    }
}

public class RedistributionAspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect2));
        return meta.Proceed();
    }
}

public class RedistributionAspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect3));
        return meta.Proceed();
    }
}

public class RedistributionAspect4 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect4));
        return meta.Proceed();
    }
}