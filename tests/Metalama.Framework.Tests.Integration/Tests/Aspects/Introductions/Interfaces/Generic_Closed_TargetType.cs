#if TEST_OPTIONS
// @Skipped(#32359)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TargetType
{
    /*
     * Tests introducing closed generic interface with a type argument set to the target type currently results in an error.
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
                ( (INamedType)TypeFactory.GetType( typeof(IInterface<>) ) ).WithTypeArguments( aspectBuilder.Target ) );
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}