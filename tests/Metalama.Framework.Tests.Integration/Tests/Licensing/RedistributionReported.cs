// @LicenseFile(Tests\Licensing\Licenses\Essentials.license)
// @DependencyLicenseFile(Tests\Licensing\Licenses\Redistribution.license)
// @Include(_Redistribution.cs);
// @Include(_Redistribution.Dependency.cs);

using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;
using Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionReported;
using System;

[assembly: AspectOrder(typeof(RedistributionAspect1), typeof(RedistributionAspect2), typeof(RedistributionAspect3), typeof(RedistributionAspect4),
    typeof(NonredistributionAspect1), typeof(NonredistributionAspect2), typeof(NonredistributionAspect3))]

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionReported;

public class NonredistributionAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect1));
        return meta.Proceed();
    }
}

public class NonredistributionAspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect2));
        return meta.Proceed();
    }
}

public class NonredistributionAspect3 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonredistributionAspect3));
        return meta.Proceed();
    }
}

class NonredistributionAspectTargetClass
{
    [NonredistributionAspect1]
    [NonredistributionAspect2]
    [NonredistributionAspect3]
    void NonredistributionAspectTargetMethod()
    {
    }
}