using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ProjectFabricConfigure
{
    internal class Fabric : IProjectFabric
    {
        public void AmendProject( IProjectAmender amender )
        {
            amender.Project.Data<Configuration>().Message = "Hello, world.";
        }
    }

    internal class Configuration : IProjectData
    {
        public string? Message { get; set; }

        public void Initialize( IProject project ) { }
    }

    internal class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( meta.Target.Project.Data<Configuration>().Message );

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private string Method2( string s ) => s;
    }
}