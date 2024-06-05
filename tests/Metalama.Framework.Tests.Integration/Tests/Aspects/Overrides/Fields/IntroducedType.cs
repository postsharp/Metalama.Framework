using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Fields.IntroducedType;

internal class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var typeResult = builder.Advice.IntroduceClass( builder.Target, "TestType" );
        var methodResult = builder.Advice.IntroduceField( typeResult.Declaration, nameof(IntroducedField) );

        builder.Advice.Override( methodResult.Declaration, nameof(OverrideTemplate) );
    }

    [Template]
    public int IntroducedField;

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