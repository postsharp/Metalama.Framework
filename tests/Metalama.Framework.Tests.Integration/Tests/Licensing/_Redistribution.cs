#if ASPECTS_IN_REDISTRIBUTION_NAMESPACE
using RedistributionTests.TargetNamespace;
#else
using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;
#endif

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