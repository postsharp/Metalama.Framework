using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Declarative
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public int IntroducedProperty_Auto { get; set; }

        [Introduce]
        public static int IntroducedProperty_Auto_Static { get; set; }

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

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
