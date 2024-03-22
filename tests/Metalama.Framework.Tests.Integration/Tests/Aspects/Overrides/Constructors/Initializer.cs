using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Initializer;

/*
 * Tests single OverrideConstructor advice with initializers.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.AddInitializer(builder.Target.Constructors.Single(), nameof(InitializerTemplate), args: new { i = 1 });
        builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 1 });
        builder.Advice.AddInitializer(builder.Target.Constructors.Single(), nameof(InitializerTemplate), args: new { i = 2 });
        builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 2 });
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
    public TargetClass()
    {
        Console.WriteLine($"This is the original constructor.");
    }
}