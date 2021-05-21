using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
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
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.IntroduceInterface(aspectBuilder.TargetDeclaration, (INamedType)aspectBuilder.TargetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType(typeof(IInterface)));
        }

        [IntroduceMethod]
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

    [TestOutput]
    [Introduction]
    public class TargetClass : BaseClass
    {
    }
}
