using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes;

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceField(builder.Target, "RT", typeof(RT));
        builder.Advice.IntroduceField(builder.Target, "NRT", typeof(RT).ToNullableType());
        builder.Advice.IntroduceField(builder.Target, "RTOCT", typeof(RTOCT));
        builder.Advice.IntroduceField(builder.Target, "NRTOCT", typeof(RTOCT).ToNullableType());
    }
}

[RunTimeOrCompileTime]
class RTOCT { }

class RT { }

// <target>
[Aspect]
class TargetClass { }
