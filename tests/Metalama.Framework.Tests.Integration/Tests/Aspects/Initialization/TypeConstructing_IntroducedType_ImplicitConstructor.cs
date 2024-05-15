#if TESTOPTIONS
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
        var introducedType = builder.Advice.IntroduceClass( builder.Target, "IntroducedType", TypeKind.Class ).Declaration;

        builder.Advice.IntroduceField( introducedType, nameof(Field) );

        builder.Advice.AddInitializer( introducedType, nameof(Template), InitializerKind.BeforeTypeConstructor );
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