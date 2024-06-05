using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.IntroducedType;

internal class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var typeResult = builder.Advice.IntroduceClass( builder.Target, "TestType" );
        var methodResult = builder.Advice.IntroduceMethod( typeResult.Declaration, nameof(IntroducedMethod) );

        builder.Advice.Override( methodResult.Declaration, nameof(OverrideTemplate) );
    }

    [Template]
    public void IntroducedMethod()
    {
        Console.WriteLine( "Introduced Method" );
    }

    [Template]
    public dynamic? OverrideTemplate()
    {
        Console.WriteLine( "Override" );

        return meta.Proceed();
    }
}

// <target>
[Aspect]
internal class Target { }