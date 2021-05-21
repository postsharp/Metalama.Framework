using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClassExplicitInterfaceImplementation
{
    /*
     * When the base class of the target type implements the introduced interface explicitly, an error should be produced, because C# does not allow calling
     * base class' explicit interface implementation.
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
        int IInterface.InterfaceMethod()
        {
            Console.WriteLine("This is original interface method.");
            return 27;
        }
    }

    [TestOutput]
    [Introduction]
    public class TargetClass : BaseClass
    {
    }
}
