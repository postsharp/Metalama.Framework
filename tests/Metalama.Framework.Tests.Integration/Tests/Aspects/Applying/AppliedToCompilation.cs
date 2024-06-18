using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation;

[assembly: MyAspect]

namespace Metalama.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation
{
    public class MyAspect : CompilationAspect
    {
        public override void BuildAspect( IAspectBuilder<ICompilation> builder )
        {
            throw new Exception( "Oops" );
        }
    }

    // <target>
    internal class TargetClass { }
}