using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Accessibility
{
    /*
     * Tests accessibility of implicit members.
     */

    public interface IInterface
    {
        void Method();
        int Property { get; set; }
        int Property_PrivateSetter { get; }
        int Property_GetOnly { get; }
        int Property_ExpressionBody { get; }
        int AutoProperty { get; set; }
        int AutoProperty_PrivateSetter { get; set; }
        event EventHandler? EventField;
        event EventHandler? Event;
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advice.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }

        [Introduce]
        private void Method()
        {
            Console.WriteLine("Introduced interface member");
        }

        [Introduce]
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

        [Introduce]
        public int Property_PrivateSetter
        {
            get
            {
                return 42;
            }

            private set
            {
            }
        }

        [Introduce]
        private int Property_GetOnly
        {
            get
            {
                return 42;
            }
        }

        [Introduce]
        private int Property_ExpressionBody => 42;

        [Introduce]
        private int AutoProperty { get; set; }

        [Introduce]
        public int AutoProperty_PrivateSetter { get; private set; }

        [Introduce]
        private event EventHandler? EventField;

        [Introduce]
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