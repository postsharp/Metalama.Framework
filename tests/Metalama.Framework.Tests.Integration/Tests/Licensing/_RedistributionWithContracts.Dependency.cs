using Metalama.Framework.Aspects;
using System;

using Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContracts.Dependency;

[assembly: AspectOrder(typeof(RedistributionAspect1), typeof(RedistributionAspect2), typeof(RedistributionContract1), typeof(RedistributionContract2))]

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContracts.Dependency;

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

public class RedistributionContract1 : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), $"Validated by {nameof(RedistributionContract1)}.");
        }
    }
}

public class RedistributionContract2 : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), $"Validated by {nameof(RedistributionContract2)}.");
        }
    }
}