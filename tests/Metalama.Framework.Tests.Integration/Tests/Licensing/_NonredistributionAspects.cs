using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.Tests.Licensing.AdditionalAspects;
using System;
using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;

[assembly: AspectOrder(
    AspectOrderDirection.RunTime,
    typeof(RedistributionAspect1),
    typeof(RedistributionAspect2),
    typeof(RedistributionAspect3),
    typeof(RedistributionAspect4),
    typeof(NonredistributionAspect1),
    typeof(NonredistributionAspect2),
    typeof(NonredistributionAspect3) )]

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AdditionalAspects;

public class NonredistributionAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect1) );

        return meta.Proceed();
    }
}

public class NonredistributionAspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect2) );

        return meta.Proceed();
    }
}

public class NonredistributionAspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect3) );

        return meta.Proceed();
    }
}

internal class NonredistributionAspectTargetClass
{
    [NonredistributionAspect1]
    [NonredistributionAspect2]
    [NonredistributionAspect3]
    private void NonredistributionAspectTargetMethod() { }
}