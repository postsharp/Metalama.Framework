using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30662;

internal class RegisterInstanceAttribute : ConstructorAspect
{
    public override void BuildAspect( IAspectBuilder<IConstructor> builder )
    {
        builder.Advice.IntroduceParameter(
            builder.Target,
            "instanceRegistry",
            typeof(IInstanceRegistry),
            TypedConstant.Default( typeof(IInstanceRegistry) ),
            pullAction: ( parameter, constructor ) =>
                PullAction.IntroduceParameterAndPull(
                    "instanceRegistry",
                    TypeFactory.GetType( typeof(IInstanceRegistry) ),
                    TypedConstant.Default( typeof(IInstanceRegistry) ) ) );

        builder.Advice.AddInitializer( builder.Target, StatementFactory.Parse( "instanceRegistry.Register( this );" ) );
    }
}

public interface IInstanceRegistry
{
    void Register( object instance );
}

internal class Foo
{
    [RegisterInstance]
    public Foo() { }
}

internal class Bar : Foo { }