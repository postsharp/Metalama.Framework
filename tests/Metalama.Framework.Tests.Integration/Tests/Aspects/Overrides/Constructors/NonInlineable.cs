using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.NonInlineable;

/*
 * Tests that single OverrideConstructor advice with non-inlineable template produces a diagnostic error.
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

        meta.Proceed();
        meta.Proceed();
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