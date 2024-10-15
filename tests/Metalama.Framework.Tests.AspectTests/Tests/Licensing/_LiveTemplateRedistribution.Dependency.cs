using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.AspectTests.Tests.Licensing.LiveTemplateRedistribution.Dependency
{
    [EditorExperience( SuggestAsLiveTemplate = true )]
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by resdistributed " + nameof(TestAspect) );

            return meta.Proceed();
        }
    }
}