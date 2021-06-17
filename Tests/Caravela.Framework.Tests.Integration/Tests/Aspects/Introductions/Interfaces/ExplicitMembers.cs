// @Skipped Linker does not yet support overrides of explicit interface members.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers
{
    /*
     * Simple case with explicit interface members for a single interface.
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

        [InterfaceMember(IsExplicit = true)]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface member.");
            return meta.Proceed();
        }

        [InterfaceMember(IsExplicit = true)]
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

        [InterfaceMember(IsExplicit = true)]
        public event EventHandler? EventField;

        [InterfaceMember(IsExplicit = true)]
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

        [InterfaceMember(IsExplicit = true)]
        public int AutoProperty { get; set; }
    }

    [TestOutput]
    [Introduction]
    public class TargetClass
    {
    }
}
