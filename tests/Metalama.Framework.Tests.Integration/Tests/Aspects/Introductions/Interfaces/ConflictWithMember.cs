// @Skipped(#30229)

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictWithMember
{
    /*
     * Member of the same name already exists in the target class for implicit interface member.
     */

    public interface IInterface
    {
        int InterfaceMethod();
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

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is introduced interface method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is the original implementation." );

            return 42;
        }
    }
}