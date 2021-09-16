using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToParameter
{
    public class MyAspect : Attribute, IAspect<IParameter> 
    { 
        public void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            throw new Exception("Oops");
        }
    }


    // <target>
    internal class TargetClass
    {
        void M([MyAspect] int a ) {}
    }
}
