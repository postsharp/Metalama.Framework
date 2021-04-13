using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverride
{
    // TODO: Will be fixed as part of #28322 Handle conflicts and overrides.

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.Override)]
        public int BaseMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.Override)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }

        [IntroduceMethod(ConflictBehavior = ConflictBehavior.Override)]
        public int NonExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return 42;
        }
    }

    internal class BaseClass
    { 
        public int BaseMethod()
        {
            return 13;
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass : BaseClass
    {
        public int ExistingMethod()
        {
            return 27;
        }
    }
}
