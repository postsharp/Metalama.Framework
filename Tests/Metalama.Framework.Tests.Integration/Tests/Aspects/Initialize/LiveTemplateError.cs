using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Initialize.LiveTemplateError
{
    internal class Aspect : MethodAspect
    {
        public Aspect( int x ) { }

        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            // This should not be allowed because there is no default constructor.
            builder.IsLiveTemplate = true;
        }

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            // This should not be called.
            throw new Exception( "Oops" );
        }
    }

    // <target>
    internal class Target
    {
        [Aspect( 0 )]
        private void M() { }
    }
}