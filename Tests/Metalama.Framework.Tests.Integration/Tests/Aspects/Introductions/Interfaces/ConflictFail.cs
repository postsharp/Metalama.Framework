﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictFail
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
                whenExists: OverrideStrategy.Fail );
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