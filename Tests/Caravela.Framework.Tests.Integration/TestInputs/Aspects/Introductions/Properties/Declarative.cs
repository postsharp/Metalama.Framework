using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) { }

        // TODO: Indexers.    

        //[IntroduceProperty]
        //public int IntroducedProperty_Auto { get; set; }


        // TODO: Introduction of auto properties.
        //[IntroduceProperty]
        //public static int IntroducedProperty_Auto_Static { get; }

        [Introduce]
        public int IntroducedProperty_Accessors
        {
            get 
            { 
                Console.WriteLine("Get"); 
                return 42; 
            }

            set 
            { 
                Console.WriteLine(value); 
            }
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
