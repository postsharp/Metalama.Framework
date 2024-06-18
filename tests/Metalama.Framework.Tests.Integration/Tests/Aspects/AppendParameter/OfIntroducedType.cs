using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.OfIntroducedType;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introducedType = builder.IntroduceClass( "X", buildType: b => { b.Accessibility = Accessibility.Public; } )
            .Declaration;

        builder.Advice.IntroduceParameter( builder.Target.Constructors.Single(), "p", introducedType, TypedConstant.Default( introducedType ) );
    }
}

// <target>
[MyAspect]
public class C
{
    public C() { }
}