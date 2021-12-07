using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Fabrics.TransitiveProjectFabric
{
    public class  Fabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender.Project.Extension<Configuration>().Message = "Configured";
        }
    }

    // <target>
    public class TargetCode
    {
        public int Method() => 0;
    }
}