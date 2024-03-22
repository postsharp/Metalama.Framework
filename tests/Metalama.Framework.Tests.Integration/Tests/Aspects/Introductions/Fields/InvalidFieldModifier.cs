using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.InvalidFieldModifier;

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceField(builder.Target, "field", typeof(int), buildField: field => field.Writeability = Writeability.InitOnly);
    }
}

// <target>
[Aspect]
class TargetClass { }