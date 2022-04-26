using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Fabrics.TransitiveProjectFabric
{
    public class TransitiveFabric : Framework.Fabrics.TransitiveProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            var configuration = amender.Project.Extension<Configuration>();

            // Capture the message outside of the lambda otherwise it gets evaluated later and we don't test that the transitive fabric runs
            // after the non-transitive one.
            var message = configuration.Message;
            amender.With( c => c.Types.SelectMany( t => t.Methods ) ).AddAspect( m => new Aspect( message ) );
        }
    }

    public class Configuration : ProjectExtension
    {
        public string Message { get; set; } = "Not Configured";
    }

    public class Aspect : OverrideMethodAspect
    {
        private string _message;

        public Aspect( string message )
        {
            _message = message;
        }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( _message );

            return meta.Proceed();
        }
    }
}