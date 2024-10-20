#if TEST_OPTIONS
// @Skipped(Constructor introduction not finished)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_IntroducedType_ImplicitConstructor;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introducedType = builder.IntroduceClass( "IntroducedType" ).Declaration;

        builder.With( introducedType ).IntroduceField( nameof(Field) );

        builder.With( introducedType ).AddInitializer( nameof(Template), InitializerKind.BeforeTypeConstructor );
    }

    [Template]
    public static int Field = 42;

    [Template]
    public void Template()
    {
        Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}" );
    }
}

// <target>
[Aspect]
public class TargetCode
{
    public static int Foo = 42;
}