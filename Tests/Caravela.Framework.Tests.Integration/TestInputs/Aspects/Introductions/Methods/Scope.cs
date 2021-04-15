using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Scope
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod(Scope = IntroductionScope.Default)]
        public int DefaultScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Default)]
        public static int DefaultScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Instance)]
        public int InstanceScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Instance)]
        public static int InstanceScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Static)]
        public int StaticScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Static)]
        public static int StaticScopeStatic()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Target)]
        public int TargetScope()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(Scope = IntroductionScope.Target)]
        public static int TargetScopeStatic()
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
