// @LicenseFile(Tests\Licensing\Licenses\Essentials.license)
// @DependencyLicenseFile(Tests\Licensing\Licenses\Redistribution.license)

using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution;

class RedistributionTargetClass
{
    [RedistributionAspect1]
    [RedistributionAspect2]
    [RedistributionAspect3]
    [RedistributionAspect4]
    void RedistributionTargetMethod()
    {
    }
}