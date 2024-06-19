using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.LiveTemplate
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
        private int TargetMethod( int a )
        {
            return a;
        }
    }
}