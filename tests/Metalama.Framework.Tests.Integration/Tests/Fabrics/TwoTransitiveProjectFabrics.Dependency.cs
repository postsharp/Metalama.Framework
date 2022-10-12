using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Fabrics.TwoTransitiveProjectFabrics
{
    public class TransitiveFabric1 : Framework.Fabrics.TransitiveProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
           
        }
    }
    
    public class TransitiveFabric2 : Framework.Fabrics.TransitiveProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
           
        }
    }

}