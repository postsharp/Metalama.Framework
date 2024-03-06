using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Contract;

/*
 * Tests single OverrideConstructor advice with contracts.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.AddContract(builder.Target.Constructors.Single().Parameters.Single(), nameof(InitializerTemplate), args: new { i = 1 });
        builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 1 });
        builder.Advice.AddContract(builder.Target.Constructors.Single().Parameters.Single(), nameof(InitializerTemplate), args: new { i = 2 });
        builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { i = 2 });
    }

    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine( $"This is the override {i}." );
        meta.Proceed();
    }

    [Template]
    public void InitializerTemplate([CompileTime] int i, dynamic? value)
    { 
        Console.WriteLine( $"This is the contract {i}." );
    }
}

// <target>
[Override]
public class TargetClass
{
    public TargetClass(int p)
    {
        Console.WriteLine($"This is the original constructor.");
    }
}