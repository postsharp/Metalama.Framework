using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Applying.Target_Struct_AppliedToParameter
{
    public class MyAspect : ParameterAspect
    {
        public override void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            throw new Exception( "Oops" );
        }
    }

    // <target>
    internal struct TargetStruct
    {
        private void M( [MyAspect] int a ) { }
    }
}