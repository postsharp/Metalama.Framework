using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Accessibility_Explicit
{
    /*
     * Tests accessibility of explicit members (should be always ignored).
     */

    public interface IInterface
    {
        void Method();
        int Property { get; set; }
        int Property_GetOnly { get; }
        int Property_ExpressionBody { get; }
        int AutoProperty { get; set; }
        event EventHandler? EventField;
        event EventHandler? Event;
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advice.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }

        [ExplicitInterfaceMember]
        private void Method()
        {
            Console.WriteLine("Introduced interface member");
        }

        [ExplicitInterfaceMember]
        private int Property
        {
            get
            {
                return 42;
            }

            set
            {
            }
        }

        [ExplicitInterfaceMember]
        private int Property_GetOnly
        {
            get
            {
                return 42;
            }
        }

        [ExplicitInterfaceMember]
        private int Property_ExpressionBody => 42;

        [ExplicitInterfaceMember]
        private int AutoProperty { get; set; }

        [ExplicitInterfaceMember]
        private event EventHandler? EventField;

        [ExplicitInterfaceMember]
        private event EventHandler? Event
        {
            add { }
            remove { }
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}