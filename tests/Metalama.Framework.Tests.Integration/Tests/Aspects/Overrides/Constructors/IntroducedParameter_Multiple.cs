#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.IntroducedParameter_Multiple;

[assembly:AspectOrder(typeof(Override2Attribute), typeof(Override1Attribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.IntroducedParameter_Multiple;

/*
 * Tests single OverrideConstructor advice with with IntroduceParameter in multiple aspects.
 */

public class Override1Attribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.Override(constructor, nameof(Template), args: new { i = 1 });
            builder.Advice.IntroduceParameter(constructor, "introduced", TypeFactory.GetType(SpecialType.Int32), TypedConstant.Create(42));
        }
    }

    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine( $"This is the override {i}." );

        foreach (var param in meta.Target.Parameters)
        {
            Console.WriteLine( $"Param {param.Name} = {param.Value}" );
        }

        meta.Proceed();
    }
}
public class Override2Attribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.Override(constructor, nameof(Template), args: new { i = 2 });
        }
    }

    [Template]
    public void Template([CompileTime] int i)
    {
        Console.WriteLine($"This is the override {i}.");

        foreach (var param in meta.Target.Parameters)
        {
            Console.WriteLine($"Param {param.Name} = {param.Value}");
        }

        meta.Proceed();
    }
}

// <target>
[Override1]
[Override2]
public class TargetClass
{
    public TargetClass()
    {
        Console.WriteLine($"This is the original constructor.");
    }

    public TargetClass(int x)
    {
        Console.WriteLine($"This is the original constructor.");
    }
}

#endif