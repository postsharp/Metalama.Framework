using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.InterfaceConflict_DerivedAfterBase_Ignore
{
    /*
     * Tests that when a single aspect introduces a base interface before the derived interface, the remainder of derived interface is implemented.
     */

    public interface IBaseInterface
    {
        int Foo();
    }

    public interface IDerivedInterface : IBaseInterface
    {
        int Bar();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IBaseInterface), tags: new { Source = "Base" } );

            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                typeof(IDerivedInterface),
                OverrideStrategy.Ignore,
                tags: new { Source = "Derived" } );
        }

        [InterfaceMember]
        public int Foo()
        {
            Console.WriteLine( $"This is introduced interface member by {meta.Tags["Source"]} (should be Base)." );

            return meta.Proceed();
        }

        [InterfaceMember]
        public int Bar()
        {
            Console.WriteLine( $"This is introduced interface member by {meta.Tags["Source"]} (should be Derived)." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}