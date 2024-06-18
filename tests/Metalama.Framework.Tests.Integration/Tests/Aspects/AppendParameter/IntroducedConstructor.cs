using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.IntroducedConstructor;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introduced = builder.IntroduceConstructor( nameof(ConstructorTemplate), buildConstructor: c => { c.AddParameter( "a", typeof(int) ); } )
            .Declaration;

        builder.Advice.IntroduceParameter( introduced, "p", typeof(int), TypedConstant.Create( 15 ) );
    }

    [Template]
    public void ConstructorTemplate() { }
}

// <target>
[MyAspect]
public class C { }