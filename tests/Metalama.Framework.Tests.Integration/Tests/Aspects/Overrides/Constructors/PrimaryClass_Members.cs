﻿#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0414

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryClass_Members;

/*
 * Tests single OverrideConstructor advice on a primary constructor of a non-record class that contains static fields, properties and events with initializers.
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
    public const int Foo = 42;

    public static int Hoo = 42;

    public static readonly int Goo = 42;

    public static int Boo { get; } = 42;

    public static event EventHandler Zoo = null!;

    public int Bar = x;
}
#endif