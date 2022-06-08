using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_SameSignature_Explicit
{
    /*
     * Tests that when a member of the same signature already exists in the target class for an explicit interface member, the compilation succeeds.
     */

    public interface IInterface
    {
        void Method();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                typeof(IInterface),
                whenExists: OverrideStrategy.Ignore );
        }

        [InterfaceMember(IsExplicit = true)]
        public void Method()
        {
            Console.WriteLine( "This is introduced interface method." );
        }
    }

    // <target>
    [Introduction]
    public class TargetClass
    { 
        public void Method()
        {
            Console.WriteLine("This is original method.");
        }
    }
}