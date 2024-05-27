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
        var introducedType = builder.Advice.IntroduceClass( builder.Target, "IntroducedType", TypeKind.Class ).Declaration;
        builder.Advice.IntroduceConstructor( introducedType, nameof(StaticConstructorTemplate), buildConstructor: b => { b.IsStatic = true; } );
        builder.Advice.AddInitializer( introducedType, nameof(Template), InitializerKind.BeforeTypeConstructor );
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