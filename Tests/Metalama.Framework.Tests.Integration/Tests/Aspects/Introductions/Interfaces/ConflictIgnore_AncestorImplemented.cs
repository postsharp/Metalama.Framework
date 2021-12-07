﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictIgnore_AncestorImplemented
{
    /*
     * Case with base interface being already implemented and fail behavior being specified.
     */

    public interface IBaseInterface
    {
        int Foo();
    }

    public interface IDerivedInterface
    {
        int Bar();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advices.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface), whenExists: OverrideStrategy.Ignore );
        }

        [InterfaceMember]
        public int Foo()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }

        [InterfaceMember]
        public int Bar()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : IBaseInterface
    {
        public int Foo()
        {
            Console.WriteLine( "This is original interface member." );

            return 13;
        }
    }
}