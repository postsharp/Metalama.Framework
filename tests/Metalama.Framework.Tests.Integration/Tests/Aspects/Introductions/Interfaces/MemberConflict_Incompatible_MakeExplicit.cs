using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_Incompatible_MakeExplicit
{
    /*
     * Tests that when an incompatible member of the same signature already exist and whenExists is set to MakeExplicit, the interface is introduced and 
     * its members are implements explicitly.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advise.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit)]
        public int Method()
        {
            Console.WriteLine("This is introduced interface method.");
            return 42;
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit)]
        public int Property
        {
            get
            {
                Console.WriteLine("This is introduced interface property.");
                return 42;
            }

            set
            {
                Console.WriteLine("This is introduced interface property.");
            }
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit)]
        public event EventHandler Event
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
        int Method();

        int Property { get; set; }

        event EventHandler Event;
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
        public string Method()
        {
            Console.WriteLine("This is original method.");
            return "42";
        }

        public string Property
        {
            get
            {
                Console.WriteLine("This is original property.");
                return "42";
            }

            set
            {
                Console.WriteLine("This is original property.");
            }
        }

        public event Action Event
        {
            add
            {
                Console.WriteLine("This is original event.");
            }

            remove
            {
                Console.WriteLine("This is original event.");
            }
        }
    }
}