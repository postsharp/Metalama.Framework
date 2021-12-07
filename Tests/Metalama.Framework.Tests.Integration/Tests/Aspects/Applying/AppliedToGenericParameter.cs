using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToGenericParameter
{
    public class MyAspect : TypeParameterAspect
    {
        public override void BuildAspect( IAspectBuilder<ITypeParameter> builder )
        {
            throw new Exception( "Oops" );
        }
    }

    // <target>
    internal class TargetClass
    {
        private void M<[MyAspect] T>( int a ) { }
    }
}