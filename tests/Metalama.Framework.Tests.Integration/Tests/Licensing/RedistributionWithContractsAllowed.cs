// @LicenseFile(Tests\Licensing\Licenses\Free.license)
// @DependencyLicenseFile(Tests\Licensing\Licenses\Redistribution.license)
// @Include(_RedistributionWithContracts.cs);
// @Include(_RedistributionWithContracts.Dependency.cs);
// @OutputAllSyntaxTrees

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContractsAllowed;

public class NonRedistributionAspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(NonRedistributionAspect1));
        return meta.Proceed();
    }
}

public class Contract1 : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), $"Validated by {nameof(Contract1)}.");
        }
    }
}

class TargetClass
{
    [NonRedistributionAspect1]
    public void TargetMethod([Contract1] int? targetParameter) { }
}