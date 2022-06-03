using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers_Override;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitMembers_Override
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

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }

        [InterfaceMember]
        public event EventHandler? Event
        {
            add
            {
                Console.WriteLine( "This is introduced interface member." );
            }

            remove
            {
                Console.WriteLine( "This is introduced interface member." );
            }
        }

        [InterfaceMember]
        public event EventHandler? EventField;

        [InterfaceMember]
        public int Property
        {
            get
            {
                Console.WriteLine( "This is introduced interface member." );

                return 42;
            }

            set
            {
                Console.WriteLine( "This is introduced interface member." );
            }
        }

        [InterfaceMember]
        public int AutoProperty { get; set; }
    }

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            foreach (var method in aspectBuilder.Target.Methods)
            {
                if (!method.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advice.Override( method, nameof(Template) );
                }
            }

            foreach (var property in aspectBuilder.Target.Properties)
            {
                if (!property.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advice.OverrideAccessors( property, nameof(Template), nameof(Template) );
                }
            }

            foreach (var method in aspectBuilder.Target.Events)
            {
                if (!method.IsExplicitInterfaceImplementation)
                {
                    aspectBuilder.Advice.OverrideAccessors( method, nameof(Template), nameof(Template), null );
                }
            }
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( "This is overridden method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    [Override]
    public class TargetClass { }
}