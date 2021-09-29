using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ErrorPublicFabric
{
    // <target>
    internal class TargetCode
    {
        public class F : ITypeFabric
        {
            public void BuildType( ITypeFabricBuilder builder ) { }
        }
    }
}