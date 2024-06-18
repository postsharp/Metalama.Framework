using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.AddToIntroducedType;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.AddToIntroducedType;

internal class Aspect1 : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introducedType = builder.IntroduceClass( "IntroducedType" ).Declaration;

        builder.Outbound.SelectMany( t => t.Types ).AddAspect<Aspect2>();
    }
}

internal class Aspect2 : TypeAspect
{
    [Introduce]
    public void Foo() { }
}

// <target>
[Aspect1]
internal class TargetCode { }