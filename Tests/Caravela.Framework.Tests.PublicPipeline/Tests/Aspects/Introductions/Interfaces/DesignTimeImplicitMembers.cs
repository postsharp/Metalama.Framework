// @DesignTime

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        int InterfaceMethod();

        event EventHandler EventField;

        event EventHandler Event;

        int AutoProperty { get; set; }

        int Property { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, (INamedType)aspectBuilder.Target.Compilation.TypeFactory.GetTypeByReflectionType(typeof(IInterface)));
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface member.");
            return meta.Proceed();
        }

        [InterfaceMember]
        public event EventHandler? EventField;

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
        public int AutoProperty { get; set; }

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
    }

    // <target>
    [Introduction]
    public partial class TargetClass
    {
    }
}
