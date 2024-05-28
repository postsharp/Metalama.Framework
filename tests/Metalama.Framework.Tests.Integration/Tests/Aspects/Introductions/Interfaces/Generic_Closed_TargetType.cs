using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TargetType
{
    /*
     * Tests introducing closed generic interface with a type argument set to the target type.
     */

    public interface IInterface<T>
    {
        void Foo( T t );
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.ImplementInterface(
                builder.Target,
                ((INamedType)TypeFactory.GetType(typeof(IInterface<>))).WithTypeArguments(builder.Target));

            builder.Advice.IntroduceMethod(builder.Target, nameof(Foo), args: new { T = builder.Target });
        }

        [Template]
        public void Foo<[CompileTime] T>(T t) { }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}