﻿using Metalama.Framework.Tests.Integration.Tests.Licensing.Redistribution.Dependency;

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