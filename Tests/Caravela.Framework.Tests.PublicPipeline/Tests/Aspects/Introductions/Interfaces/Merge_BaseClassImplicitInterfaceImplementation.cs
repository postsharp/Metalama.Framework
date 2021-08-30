// @Skipped(Case for interface merge conflict resolution, not implemented.)

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClassImplicitInterfaceImplementation
{
    /*
     * When the base class of the target type implements the introduced interface implicitly, the transformed code should call the base class.
     */

    public interface IInterface
    {
        int InterfaceMethod();
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.ImplementInterface(aspectBuilder.Target, (INamedType)aspectBuilder.Target.Compilation.TypeFactory.GetTypeByReflectionType(typeof(IInterface)));
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
        }

        [Introduce]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }
    }

    public class BaseClass : IInterface
    {
        public int InterfaceMethod()
        {
            return 27;
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : BaseClass
    {
    }
}
