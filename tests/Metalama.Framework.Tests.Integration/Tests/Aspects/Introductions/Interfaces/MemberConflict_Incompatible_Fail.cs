using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_Incompatible_Fail
{
    /*
     * Tests that when a incompatible member of the same signature already exists and whenExists is set to Fail, errors is produced.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                typeof(IInterface),
                whenExists: OverrideStrategy.Ignore );
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Fail)]
        public object Method()
        {
            Console.WriteLine( "This is introduced interface method." );
            return new object();
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Fail)]
        public object Property
        {
            get
            {
                Console.WriteLine("This is introduced interface property.");
                return new object();
            }

            set
            {
                Console.WriteLine("This is introduced interface property.");
            }
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Fail)]
        public event Action Event
        {
            add
            {
                Console.WriteLine("This is introduced interface event.");
            }

            remove
            {
                Console.WriteLine("This is introduced interface event.");
            }
        }
    }

    public interface IInterface
    {
        void Method();

        int Property { get; }

        event EventHandler Event;
    }

    // <target>
    [Introduction]
    public class TargetClass
    { 
        public int Method( int x )
        {
            Console.WriteLine("This is original method.");
            return x;
        }

        public int Property { get; set; }

        public event EventHandler? Event;
    }
}