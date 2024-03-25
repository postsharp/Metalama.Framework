using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_Positional
{
    internal class MyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed();
            set => meta.Proceed();
        }
    }

    // <target>
    internal record MyRecord( int A, int B );

    internal class Fabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender.Outbound.SelectMany( p => p.Types.OfName( "MyRecord" ).SelectMany( t => t.Properties.Where( p => !p.IsImplicitlyDeclared ) ) )
                .AddAspect<MyAspect>();
        }
    }
}