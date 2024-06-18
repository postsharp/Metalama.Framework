﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryRecordStruct_ExplicitProperty;

/*
 * Tests single OverrideConstructor advice on a primary constructor of a record struct with a explicitly defined positional property.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            if (constructor.IsImplicitlyDeclared)
            {
                continue;
            }

            builder.Advice.Override( constructor, nameof(Template) );
        }
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is the override." );

        foreach (var param in meta.Target.Parameters)
        {
            Console.WriteLine( $"Param {param.Name} = {param.Value}" );
        }

        meta.Proceed();
    }
}

// <target>
[Override]
public record struct TargetStruct( int X, int Y )
{
    public int X { get; set; } = X;

    public void Foo()
    {
        X = 42;
        var (x, y) = this;
        _ = this with { X = 13, Y = 42 };
    }
}