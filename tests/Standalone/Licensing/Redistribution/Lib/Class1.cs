using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Lib;
public class Fabric : TransitiveProjectFabric
{
    public override void AmendProject(IProjectAmender amender)
    {
        amender.Outbound.SelectMany(c => c.AllTypes).AddAspectIfEligible<RedistributedAspect>();
    }
}

public class RedistributedAspect : TypeAspect
{
    [Introduce]
    public void RedistributionMethod()
    {
    }
}
