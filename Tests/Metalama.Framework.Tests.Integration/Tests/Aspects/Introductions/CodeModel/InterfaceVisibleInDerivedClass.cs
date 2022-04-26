﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.CodeModel.InterfaceVisibleInDerivedClass
{
    /*
     * Tests that the aspect on the derived class sees the introduced interface on the base class.
     */

    public interface IInterface { }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            if (!aspectBuilder.Target.AllImplementedInterfaces.Contains(typeof(IInterface)))
            {
                aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
            }
        }
    }

    // <target>
    [Introduction]
    public class BaseClass { }

    // <target>
    [Introduction]
    public class DerivedClass : BaseClass { }
}