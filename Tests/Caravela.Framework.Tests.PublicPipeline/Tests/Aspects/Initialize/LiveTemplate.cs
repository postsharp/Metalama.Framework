using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.LiveTemplate
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            // This should be allowed.
            builder.IsLiveTemplate = true;
        }
    }

    // <target>
    internal class T { }
}