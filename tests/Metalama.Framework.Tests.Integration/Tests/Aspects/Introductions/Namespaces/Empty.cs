using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Empty;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceNamespace( builder.Target.Compilation.GlobalNamespace, "Implementations" );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }