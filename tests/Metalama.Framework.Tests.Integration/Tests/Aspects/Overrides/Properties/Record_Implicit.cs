using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_Implicit
{
    internal class MyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty { get => meta.Proceed(); set => meta.Proceed(); }
    }

    internal record MyRecord( int A, int B );

    internal class Fabric : ProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            amender.With(p => p.Types.SelectMany(t => t.Properties)).AddAspectIfEligible<MyAspect>();
        }
    }

}
