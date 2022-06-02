using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseAfterDerived
{
    /*
     * Case with base interface being introduced after the derived interface (which is invalid).
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
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface) );
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IBaseInterface), OverrideStrategy.Ignore );
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
    public class TargetClass { }
}