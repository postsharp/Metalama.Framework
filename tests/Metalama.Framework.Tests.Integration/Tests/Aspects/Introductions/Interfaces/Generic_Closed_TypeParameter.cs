using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TypeParameter
{
    /*
     * Tests introducing closed generic type with a type parameter type argument.
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
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments( aspectBuilder.Target.TypeParameters[0] ) );

            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments(
                    TypeFactory.MakeArrayType( aspectBuilder.Target.TypeParameters[0] ) ) );

            aspectBuilder.Advice.ImplementInterface(
                aspectBuilder.Target,
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) )
                .WithTypeArguments(
                    ( (INamedType)TypeFactory.GetType( typeof(Tuple<,>) ) )
                    .WithTypeArguments( aspectBuilder.Target.TypeParameters[0], aspectBuilder.Target.TypeParameters[0] ) ) );
        }
    }

    // <target>
    [Introduction]
    public class TargetClass<T> { }
}