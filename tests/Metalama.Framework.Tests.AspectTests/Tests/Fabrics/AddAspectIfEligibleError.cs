using Metalama.Framework.Aspects;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using System;

namespace Metalama.Framework.Tests.AspectTests.Tests.Fabrics.AddAspectIfEligibleError;

class MyAspect : TypeAspect
{
    [Template]
    ref int Method(ref int x) => ref x;
}

class Fabric : ProjectFabric
{
    public override void AmendProject(IProjectAmender amender)
    {
        amender.SelectTypes().AddAspectIfEligible<MyAspect>();
    }
}

// <target>
public class Target { }