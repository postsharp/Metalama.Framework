using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Fabrics.TransitiveProjectFabric
{
    public class Fabric : IProjectFabric
    {
        public void AmendProject( IProjectAmender builder )
        {
            builder.Project.Data<Configuration>().Message = "Configured";
        }
    }

    // <target>
    public class TargetCode
    {
        public int Method() => 0;
    }
}