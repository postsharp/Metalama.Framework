using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.InterfaceConflict_DerivedAfterBase_Fail
{
    /*
     * Tests that when a single aspect introduces a base interface before the derived interface, the remainder of derived interface is implemented.
     */

    public interface IBaseInterface
    {
        int Foo();
    }

    public interface IDerivedInterface : IBaseInterface
    {
        int Bar();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IBaseInterface) );
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface), OverrideStrategy.Fail );
        }

        [Introduce]
        public int Foo()
        {
            return meta.Proceed();
        }

        [Introduce]
        public int Bar()
        {
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}