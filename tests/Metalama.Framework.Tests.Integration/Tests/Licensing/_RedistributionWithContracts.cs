using Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContracts.Dependency;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContracts;

class RedistributionTargetClass
{
    [RedistributionAspect1]
    [RedistributionAspect2]
    void RedistributionTargetMethod([RedistributionContract1][RedistributionContract2] int? targetParameter)
    {
    }
}