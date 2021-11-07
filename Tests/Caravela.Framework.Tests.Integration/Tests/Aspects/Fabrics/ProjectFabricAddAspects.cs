using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ProjectFabricAddAspects
{
    internal class Fabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender
                .WithMembers( c => c.Types.SelectMany( t => t.Methods ).Where( m => m.ReturnType.Is( typeof(string) ) ) )
                .AddAspect<Aspect>();
        }
    }

    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "overridden" );
            Console.WriteLine( ((IFabricInstance) meta.AspectInstance.Predecessors.Single().Instance).Fabric.ToString() );

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetCode
    {
        private int Method1( int a ) => a;

        private string Method2( string s ) => s;
    }
}