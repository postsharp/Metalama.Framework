using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable;

#pragma warning disable CS0067

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroductionAttribute))]

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ExplicitMembers_Override_NotInlineable
{
    /*
     * Simple case with explicit interface members for a single interface.
     */

    public interface IInterface
    {
        int InterfaceMethod();

        event EventHandler? Event;

        event EventHandler? EventField;

        int Property { get; set; }

        int AutoProperty { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
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
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced interface member.");
                meta.Proceed();
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
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("This is introduced interface member.");
                meta.Proceed();
            }
        }

        [InterfaceMember(IsExplicit = true)]
        public int AutoProperty { get; set; }
    }

    public class OverrideAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            foreach (var method in aspectBuilder.Target.Methods)
            {
                if (method.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advices.OverrideMethod(method, nameof(Template));
                }
            }

            foreach (var property in aspectBuilder.Target.Properties)
            {
                if (property.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advices.OverrideFieldOrPropertyAccessors(property, nameof(Template), nameof(Template));
                }
            }

            foreach (var method in aspectBuilder.Target.Events)
            {
                if (method.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advices.OverrideEventAccessors(method, nameof(Template), nameof(Template), null);
                }
            }
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine("This is overridden method.");
            _ = meta.Proceed();
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    [Override]
    public class TargetClass
    {
    }
}
