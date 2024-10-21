using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.AspectTests.Tests.Licensing.LiveTemplate
{
    [EditorExperience( SuggestAsLiveTemplate = true )]
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( meta.Target.Method.ToDisplayString() + " enhanced by " + nameof(TestAspect) );

            return meta.Proceed();
        }
    }

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