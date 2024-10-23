﻿using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.InterfaceConflict_BaseAfterDerived_Fail
{
    /*
     * Tests that when a single aspect introduces a base interface after the derived interface and whenExists is Fail, an error is emitted.
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
            aspectBuilder.ImplementInterface( typeof(IDerivedInterface) );
            aspectBuilder.ImplementInterface( typeof(IBaseInterface), OverrideStrategy.Fail );
        }

        [InterfaceMember]
        public int Foo()
        {
            return meta.Proceed();
        }

        [InterfaceMember]
        public int Bar()
        {
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}