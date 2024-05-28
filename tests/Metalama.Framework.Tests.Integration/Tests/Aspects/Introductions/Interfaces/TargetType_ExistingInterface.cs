using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.TargetType_ExistingInterface
{
    /*
     * Tests that target implementing another interface does not interfere with interface introduction.
     */

    public class IntroduceAspectAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advice.ImplementInterface(aspectBuilder.Target, typeof(IIntroducedInterface));
        }

        [Introduce]
        public void IntroducedMethod()
        {
            Console.WriteLine("Introduced interface member.");
        }
    }

    public interface IExistingInterface
    {
        void ExistingMethod();
    }

    public interface IIntroducedInterface
    {
        void IntroducedMethod();
    }

    // <target>
    [IntroduceAspect]
    public class TestClass : IExistingInterface
    {
        public void ExistingMethod()
        {
            Console.WriteLine("Original interface member.");
        }
    }
}
