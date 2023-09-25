using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.MiscSkipAspect;

[assembly: AspectOrder( typeof(IsSkippedAspect), typeof(SkippedAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.MiscSkipAspect;

public class SkippedAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
       builder.SkipAspect();
    }
}

public class IsSkippedAspect : TypeAspect
{
    [Introduce]
    public static bool IsSkipped 
       = meta.Target.Type.Enhancements().GetAspectInstances().Where( a => a.Aspect is SkippedAspect ).Single().IsSkipped;
}

// <target>
[SkippedAspect, IsSkippedAspect]
public class C {}