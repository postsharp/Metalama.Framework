using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Implicit_Struct;

/*
 * Tests single OverrideConstructor advice on an implicit constructor of a struct.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template));
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is the override." );
        meta.Proceed();
    }
}

// <target>
[Override]
public struct TargetStruct
{
}