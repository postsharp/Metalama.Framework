using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System.Linq;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_Implicit
{
    internal class MyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed();
            set => meta.Proceed();
        }
    }

    internal record MyRecord( int A, int B );

    internal class Fabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender.Outbound.SelectMany( p => p.Types.Where( t => t.ExecutionScope == ExecutionScope.RunTime ).SelectMany( t => t.Properties ) ).AddAspect<MyAspect>();
        }
    }
}