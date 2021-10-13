using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Fabrics.TransitiveProjectFabric
{
    public class TransitiveFabric : ITransitiveProjectFabric
    {
        public void AmendProject( IProjectAmender builder )
        {
            var configuration = builder.Project.Data<Configuration>();

            // Capture the message outside of the lambda otherwise it gets evaluated later.
            var message = configuration.Message;
            builder.WithMembers( c => c.Types.SelectMany( t => t.Methods ) ).AddAspect( m => new Aspect( message ) );
        }
    }

    public class Configuration : IProjectData
    {
        public string Message { get; set; } = "Not Configured";

        public void Initialize( IProject project ) { }
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