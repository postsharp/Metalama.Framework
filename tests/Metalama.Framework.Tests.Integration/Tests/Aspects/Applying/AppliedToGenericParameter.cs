using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Applying.AppliedToGenericParameter
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

    // <target>
    internal record TargetRecord
    {
        private void M<[MyAspect] T>(int a) { }
    }

    // <target>
    internal struct TargetStrict
    {
        private void M<[MyAspect] T>(int a) { }
    }
}