using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.InterfaceConflict_BaseAfterDerived_Ignore
{
    /*
     * Tests that when a single aspect introduces a base interface after the derived interface and whenExists is Ignore, the interface is ignored.
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
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface));
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IBaseInterface), OverrideStrategy.Ignore);
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