using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.InvalidTarget
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       

       
        
        public void BuildAspectClass( IAspectClassBuilder builder ) { }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Introduction]
        void Method()
        {
        }
    }
}
