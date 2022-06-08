using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictFail_AncestorImplemented
{
    /*
     * Tests that when base interface being already implemented and fail behavior being specified, an error is emitted.
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
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface), whenExists: OverrideStrategy.Fail );
        }

        [InterfaceMember]
        public int Foo()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }

        [InterfaceMember]
        public int Bar()
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : IBaseInterface
    {
        public int Foo()
        {
            Console.WriteLine( "This is original interface member." );

            return 13;
        }
    }
}