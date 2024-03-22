﻿#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryClass;

/*
 * Tests single OverrideConstructor advice on a primary constructor of a non-record class.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.Override(constructor, nameof(Template));
        }
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is the override." );

        foreach (var param in meta.Target.Parameters)
        {
            Console.WriteLine($"Param {param.Name} = {param.Value}");
        }

        meta.Proceed();
    }
}

#pragma warning disable CS9113 // Parameter is unread.

// <target>
[Override]
public class TargetClass(int x, int y)
{
}
#endif