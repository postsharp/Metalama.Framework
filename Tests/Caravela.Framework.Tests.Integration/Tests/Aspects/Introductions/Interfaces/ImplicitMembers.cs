using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        int InterfaceMethod();

        event EventHandler Event;

        //int Property { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.IntroduceInterface(aspectBuilder.TargetDeclaration, (INamedType)aspectBuilder.TargetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType(typeof(IInterface)));
        }

        [Introduce]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }

        [Introduce]
        public event EventHandler? Event;

        //[Introduce]
        //public int Property { get; set; }
    }

    [TestOutput]
    [Introduction]
    public class TargetClass
    {
    }
}
