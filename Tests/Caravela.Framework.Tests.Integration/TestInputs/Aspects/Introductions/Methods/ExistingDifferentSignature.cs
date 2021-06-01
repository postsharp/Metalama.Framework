using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingDifferentSignature
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder ) { }

       
        
        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        [Introduce]
        public int ExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );
            return 42;
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
        public int ExistingMethod(int x)
        {
            return x;
        }
    }
}
