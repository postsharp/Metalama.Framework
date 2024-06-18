using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Events.IntroducedType;

internal class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var typeResult = builder.IntroduceClass( "TestType" );
        var methodResult = builder.Advice.IntroduceEvent( typeResult.Declaration, nameof(IntroducedEvent) );

        builder.Advice.OverrideAccessors( methodResult.Declaration, nameof(OverrideTemplate), nameof(OverrideTemplate) );
    }

    [Template]
    public event EventHandler IntroducedEvent
    {
        add
        {
            Console.WriteLine( "Introduced" );
        }

        remove
        {
            Console.WriteLine( "Introduced" );
        }
    }

    [Template]
    public void OverrideTemplate()
    {
        Console.WriteLine( "Override" );
        meta.Proceed();
    }
}

// <target>
[Aspect]
internal class Target { }