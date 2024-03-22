#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.IntroducedParameter_PrimaryClass;

/*
 * Tests OverrideConstructor advice with IntroduceParameter on a class with primary constructor.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.Override(builder.Target.Constructors.Single(p => !p.IsImplicitlyDeclared), nameof(Template), args: new { i = 1 });
        builder.Advice.IntroduceParameter(builder.Target.Constructors.Single(p => !p.IsImplicitlyDeclared), "introduced", TypeFactory.GetType(SpecialType.Int32), TypedConstant.Create(42));
        builder.Advice.Override(builder.Target.Constructors.Single(p => !p.IsImplicitlyDeclared), nameof(Template), args: new { i = 2 });
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

// <target>
[Override]
public class TargetClass(int x)
{
    public int Z = x;
}

#endif