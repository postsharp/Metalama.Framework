using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToGenericParameter
{
    public class MyAspect : Attribute, IAspect<IGenericParameter> 
    { 
        public void BuildAspect( IAspectBuilder<IGenericParameter> builder )
        {
            throw new Exception("Oops");
        }
    }


    // <target>
    internal class TargetClass
    {
        void M<[MyAspect] T>(int a ) {}
    }
}
