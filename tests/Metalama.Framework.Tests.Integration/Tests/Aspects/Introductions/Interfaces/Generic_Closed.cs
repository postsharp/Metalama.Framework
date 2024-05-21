using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed
{
    /*
     * Tests introducing closed generic type with a concrete type argument.
     */

    public interface IInterface<T>
    {
        void Foo( T t );
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments( TypeFactory.GetType( SpecialType.Int32 ) ) );

            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments( TypeFactory.GetType( typeof(int[]) ) ) );

            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments( TypeFactory.GetType( typeof(Tuple<int, int>) ) ) );
        }

        [Introduce]
        public void Foo( int t ) { }

        [Introduce]
        public void Foo( int[] t ) { }

        [Introduce]
        public void Foo( Tuple<int, int> t ) { }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}