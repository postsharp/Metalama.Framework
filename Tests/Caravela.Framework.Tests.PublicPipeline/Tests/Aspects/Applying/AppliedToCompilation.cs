using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation;

[assembly: MyAspect]

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation
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