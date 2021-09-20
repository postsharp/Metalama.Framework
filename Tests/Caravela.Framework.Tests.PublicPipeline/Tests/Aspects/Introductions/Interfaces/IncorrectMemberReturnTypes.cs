using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IncorrectMemberReturnTypes
{
    /*
     * Case with missing interface members.
     */

    public interface IInterface
    {
        int Method();

        event EventHandler Event;

        int Property { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.ImplementInterface(aspectBuilder.Target, typeof(IInterface));            
        }

        [InterfaceMember]
        public long Method()
        {
            Console.WriteLine("This is introduced interface member.");
            return meta.Proceed();
        }

        [InterfaceMember]
        public event UnhandledExceptionEventHandler? Event
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
        public long Property
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
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
    }
}
