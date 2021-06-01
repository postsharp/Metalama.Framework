using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Visibility
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       

       
        
        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        [Introduce(Accessibility = Accessibility.Private)]
        public int Private()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Accessibility = Accessibility.ProtectedInternal)]
        public int ProtectedInternal()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Accessibility = Accessibility.PrivateProtected)]
        public int PrivateProtected()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Accessibility = Accessibility.Internal)]
        public int Internal()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Accessibility = Accessibility.Protected)]
        public int Protected()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [Introduce(Accessibility = Accessibility.Public)]
        public int Public()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
