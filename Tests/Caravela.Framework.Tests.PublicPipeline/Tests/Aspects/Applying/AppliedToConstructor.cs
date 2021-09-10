using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToConstructor
{
    public class MyAspect : Attribute, IAspect<IConstructor> 
    { 
        public void BuildAspect( IAspectBuilder<IConstructor> builder )
        {
            throw new Exception("Oops");
        }
    }


    // <target>
    internal class TargetClass
    {
        [MyAspect]
        TargetClass()
        {
        }
    }
}
