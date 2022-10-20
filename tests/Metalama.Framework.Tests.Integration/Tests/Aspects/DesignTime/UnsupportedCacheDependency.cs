#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.UnsupportedCacheDependency
{
    public class MyAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var t in builder.Target.Namespace.Types) { }
        }
    }

    // <target>
    internal class Target
    {
        [MyAspect]
        private partial class Nested { }
    }
}