using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Visibility
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod(Accessibility = Accessibility.Private)]
        public int Private()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.ProtectedOrInternal)]
        public int ProtectedInternal()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Accessibility = Accessibility.ProtectedAndInternal)]
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
