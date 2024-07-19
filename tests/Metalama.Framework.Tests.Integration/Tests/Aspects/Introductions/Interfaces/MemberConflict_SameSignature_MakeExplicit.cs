using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_SameSignature_MakeExplicit
{
    /*
     * Tests that when a member of the same signature already exists in the target class for an implicit interface member, an error is emitted.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.ImplementInterface( typeof(IInterface) );
        }

        [InterfaceMember( WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit )]
        public int Method()
        {
            Console.WriteLine( "This is introduced interface method." );

            return 42;
        }

        [InterfaceMember( WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit )]
        public int Property
        {
            get
            {
                Console.WriteLine( "This is introduced interface property." );

                return 42;
            }

            set
            {
                Console.WriteLine( "This is introduced interface property." );
            }
        }

        [InterfaceMember( WhenExists = InterfaceMemberOverrideStrategy.MakeExplicit )]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine( "This is introduced interface event." );
            }

            remove
            {
                Console.WriteLine( "This is introduced interface event." );
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
        public int Method()
        {
            Console.WriteLine( "This is original method." );

            return 0;
        }

        public int Property
        {
            get
            {
                Console.WriteLine( "This is original property." );

                return 0;
            }

            set
            {
                Console.WriteLine( "This is original property." );
            }
        }

        public event EventHandler Event
        {
            add
            {
                Console.WriteLine( "This is original event." );
            }

            remove
            {
                Console.WriteLine( "This is original event." );
            }
        }
    }
}