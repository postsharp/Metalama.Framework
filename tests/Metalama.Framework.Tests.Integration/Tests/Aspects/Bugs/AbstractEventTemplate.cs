using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.AbstractEventTemplate;

public abstract class AbstractAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.IntroduceEvent( nameof(IntroducedEvent) );
    }

    [Template]
    public abstract event EventHandler<dynamic> IntroducedEvent;
}

public class DerivedAspect : AbstractAspect
{
    public override event EventHandler<dynamic> IntroducedEvent
    {
        add => throw new NotSupportedException();
        remove => throw new NotSupportedException();
    }
}

// <target>
[DerivedAspect]
internal class Target { }