#if TEST_OPTIONS
// @Skipped(Static constructor introduction)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_IntroducedType_ExplicitConstructor;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introducedType = builder.IntroduceClass( "IntroducedType" ).Declaration;
        builder.With( introducedType ).IntroduceConstructor( nameof(StaticConstructorTemplate), buildConstructor: b => { b.IsStatic = true; } );
        builder.With( introducedType ).AddInitializer( nameof(Template), InitializerKind.BeforeTypeConstructor );
    }

    [Template]
    public void StaticConstructorTemplate()
    {
        Console.WriteLine( $"Static constructor" );
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}" );
    }
}

// <target>
[Aspect]
public class TargetCode { }