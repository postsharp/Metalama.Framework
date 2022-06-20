using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_Incompatible_UseExisting
{
    /*
     * Tests that when a member of the same name already exists in the target class for an implicit interface member, the compilation succeeds.
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

        [InterfaceMember]
        public void Method()
        {
            Console.WriteLine( "This is introduced interface method." );
        }
    }

    public interface IInterface
    {
        void Method();
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
    }
}