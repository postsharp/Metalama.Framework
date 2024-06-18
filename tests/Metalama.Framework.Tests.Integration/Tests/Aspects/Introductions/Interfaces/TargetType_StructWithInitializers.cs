#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TargetType_StructWithInitializers
{
    /*
     * Tests that target being a struct does not interfere with interface introduction.
     */

    public class IntroduceAspectAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [InterfaceMember]
        public int AutoProperty { get; set; }

        [InterfaceMember]
        public int Property
        {
            get
            {
                Console.WriteLine( "Introduced interface member" );

                return 42;
            }

            set
            {
                Console.WriteLine( "Introduced interface member" );
            }
        }

        [InterfaceMember]
        public void IntroducedMethod()
        {
            Console.WriteLine( "Introduced interface member" );
        }

        [InterfaceMember]
        public event EventHandler? Event
        {
            add
            {
                Console.WriteLine( "Introduced interface member" );
            }

            remove
            {
                Console.WriteLine( "Introduced interface member" );
            }
        }

        [InterfaceMember]
        public event EventHandler? EventField;
    }

    public interface IInterface
    {
        int AutoProperty { get; set; }

        int Property { get; set; }

        void IntroducedMethod();

        event EventHandler? Event;

        event EventHandler? EventField;
    }

    // <target>
    [IntroduceAspect]
    public struct TargetStruct
    {
        public TargetStruct() { }

        public int ExistingField = 42;

        public int ExistingProperty { get; set; } = 42;

        public void ExistingMethod()
        {
            Console.WriteLine( "Original struct member" );
        }
    }
}