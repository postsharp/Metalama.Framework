using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.SameMethodName
{
    /*
     * Member of the same name already exists in the target class for implicit interface member.
     */

    public interface IInterface
    {
        int Method();

        int Method( int a );
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
        public int Method()
        {
            Console.WriteLine( "This is introduced interface method." );

            return 0;
        }

        [InterfaceMember]
        public int Method( int a )
        {
            Console.WriteLine( "This is introduced interface method." );

            return a;
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}