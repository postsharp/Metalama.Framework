using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.IntroducedType;

internal class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var typeResult = builder.IntroduceClass( "TestType" );
        var methodResult = builder.Advice.IntroduceProperty( typeResult.Declaration, nameof(IntroducedProperty) );

        builder.Advice.Override( methodResult.Declaration, nameof(OverrideTemplate) );
    }

    [Template]
    public int IntroducedProperty { get; set; }

    [Template]
    public dynamic? OverrideTemplate
    {
        get
        {
            Console.WriteLine( "Override" );

            return meta.Proceed();
        }

        set
        {
            Console.WriteLine( "Override" );
            meta.Proceed();
        }
    }
}

// <target>
[Aspect]
internal class Target { }