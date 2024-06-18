using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;
using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;

[assembly:
    AspectOrder(
        AspectOrderDirection.RunTime,
        typeof(RedistributionAspect1),
        typeof(RedistributionAspect2),
        typeof(RedistributionAspect3),
        typeof(RedistributionAspect4) )]

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;

public class RedistributionAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect1) );

        return meta.Proceed();
    }
}

public class RedistributionAspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect2) );

        return meta.Proceed();
    }
}

public class RedistributionAspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect3) );

        return meta.Proceed();
    }
}

public class RedistributionAspect4 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(RedistributionAspect4) );

        return meta.Proceed();
    }
}