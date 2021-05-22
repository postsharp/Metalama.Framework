using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Declarative
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        [IntroduceMethod]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine("This is introduced method.");
            var nic = meta.Proceed();
        }

        [IntroduceMethod]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [IntroduceMethod]
        public int IntroducedMethod_Param(int x)
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [IntroduceMethod]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }

        [IntroduceMethod(IsVirtual = true)]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
