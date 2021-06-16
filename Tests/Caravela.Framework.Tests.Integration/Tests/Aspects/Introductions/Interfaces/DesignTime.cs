// @DesignTime

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTime
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        int InterfaceMethod();

        event EventHandler Event;

        int Property { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.IntroduceInterface(aspectBuilder.TargetDeclaration, (INamedType)aspectBuilder.TargetDeclaration.Compilation.TypeFactory.GetTypeByReflectionType(typeof(IInterface)));
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }

        [InterfaceMember]
        public event EventHandler? Event;

        [InterfaceMember]
        public int Property { get; set; }
    }

    // <target>
    [Introduction]
    public partial class TargetClass
    {
    }
}
