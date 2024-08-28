using Metalama.Framework.Tests.Integration.Tests.Licensing.LiveTemplateRedistribution.Dependency;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.LiveTemplateRedistribution
{
    // <target>
    internal class TargetClass
    {
        [TestLiveTemplate( typeof(TestAspect) )]
        private int TargetMethod( int a )
        {
            return a;
        }
    }
}