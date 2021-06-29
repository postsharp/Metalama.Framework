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

        event EventHandler EventField;

        int Property { get; set; }

        int AutoProperty { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.IntroduceInterface(aspectBuilder.TargetDeclaration, typeof(IInterface));
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface member.");
            return meta.Proceed();
        }

        [InterfaceMember]
        public event EventHandler? Event
        {
            add
            {
                Console.WriteLine("This is introduced interface member.");
            }

            remove
            {
                Console.WriteLine("This is introduced interface member.");
            }
        }

        [InterfaceMember]
        public event EventHandler? EventField;

        [InterfaceMember]
        public int Property
        {
            get
            {
                Console.WriteLine("This is introduced interface member.");
                return 42;
            }

            set
            {
                Console.WriteLine("This is introduced interface member.");
            }
        }

        [InterfaceMember]
        public int AutoProperty { get; set; }
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
    }
}
