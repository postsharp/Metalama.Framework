using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Visibility
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) { }

        [IntroduceMethod(Accessibility = Accessibility.Private)]
        public int Private()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.ProtectedInternal)]
        public int ProtectedInternal()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.PrivateProtected)]
        public int PrivateProtected()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.Internal)]
        public int Internal()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.Protected)]
        public int Protected()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.Public)]
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
