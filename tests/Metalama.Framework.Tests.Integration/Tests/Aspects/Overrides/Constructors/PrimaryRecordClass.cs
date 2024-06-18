﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.PrimaryRecordClass;

/*
 * Tests single OverrideConstructor advice on a primary constructor of a record class.
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
public record class TargetClass( int X, int Y )
{
    public void Foo()
    {
        var (x, y) = this;
        _ = this with { X = 13, Y = 42 };
    }
}