using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ErrorPublicFabric
{
    // <target>
    internal class TargetCode
    {
        public class F : TypeFabric
        {
            public override void AmendType( ITypeAmender amender ) { }
        }
    }
}