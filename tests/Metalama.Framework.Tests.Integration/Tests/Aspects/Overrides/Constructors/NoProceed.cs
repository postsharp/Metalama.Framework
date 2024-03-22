﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.NoProceed;

/*
 * Tests single OverrideConstructor advice with no meta.Proceed call.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.Override( builder.Target.Constructors.Single(), nameof(Template) );
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is the override." );
    }
}

// <target>
[Override]
internal class TargetClass
{
    public TargetClass()
    {
        Console.WriteLine( $"This is the original constructor." );
    }
}