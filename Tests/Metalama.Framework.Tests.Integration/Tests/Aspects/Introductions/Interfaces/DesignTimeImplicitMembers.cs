#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.DesignTimeImplicitMembers
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

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                (INamedType)TypeFactory.GetType( typeof(IInterface) ) );
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }

        [InterfaceMember]
        public event EventHandler? EventField;

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
        public int AutoProperty { get; set; }

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
    }

    // <target>
    [Introduction]
    public partial class TargetClass { }
}