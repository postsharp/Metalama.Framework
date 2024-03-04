using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Initializer_Static;

/*
 * Tests single OverrideConstructor advice on a static constructor with static initializers.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.AddInitializer(builder.Target, nameof(InitializerTemplate), kind: Advising.InitializerKind.BeforeTypeConstructor, args: new { i = 1 });
        builder.Advice.Override(builder.Target.StaticConstructor, nameof(Template), args: new { i = 1 });
        builder.Advice.AddInitializer(builder.Target, nameof(InitializerTemplate), kind: Advising.InitializerKind.BeforeTypeConstructor, args: new { i = 2 });
        builder.Advice.Override(builder.Target.StaticConstructor, nameof(Template), args: new { i = 2 });
    }

    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine( $"This is the override {i}." );
        meta.Proceed();
    }

    [Template]
    public void InitializerTemplate([CompileTime] int i)
    { 
        Console.WriteLine( $"This is the initializer {i}." );
    }
}

// <target>
[Override]
public class TargetClass
{
    static TargetClass()
    {
        Console.WriteLine($"This is the original constructor.");
    }
}