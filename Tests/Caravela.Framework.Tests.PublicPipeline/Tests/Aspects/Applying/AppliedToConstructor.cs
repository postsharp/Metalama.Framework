using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToConstructor
{
    public class MyAspect : ConstructorAspect
    {
        public override void BuildAspect( IAspectBuilder<IConstructor> builder )
        {
            throw new Exception( "Oops" );
        }
    }

    // <target>
    internal class TargetClass
    {
        [MyAspect]
        private TargetClass() { }
    }
}