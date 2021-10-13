﻿using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictIgnore
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        int InterfaceMethod();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advices.ImplementInterface(
                aspectBuilder.Target,
                typeof(IInterface),
                whenExists: OverrideStrategy.Ignore );
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is introduced interface method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : IInterface
    {
        int IInterface.InterfaceMethod()
        {
            Console.WriteLine( "This is the original implementation." );

            return 42;
        }
    }
}