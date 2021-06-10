﻿// @Skipped

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExistingExplicitInterfaceImplementation
{
    /*
     * When the target class already explicitly implements the introduced interface (or it's subinterface), the explicit implementation should be overridden.
     */

    public interface ISubInterface
    {
        int SubInterfaceMethod();
    }

    public interface ISuperInterface
    {
        int SuperInterfaceMethod();
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.IntroduceInterface(aspectBuilder.TargetDeclaration, (INamedType)aspectBuilder.TargetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType(typeof(ISuperInterface)));
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
        }

        [Introduce]
        public int SubInterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }

        [Introduce]
        public int SuperInterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }
    }


    [TestOutput]
    [Introduction]
    public class TargetClass : ISubInterface
    {
        int ISubInterface.SubInterfaceMethod()
        {
            Console.WriteLine("This is original interface method.");
            return 27;
        }
    }
}
