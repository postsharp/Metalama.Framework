#if TEST_OPTIONS
// @ForbiddenConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Implicit_RecordStruct_CSharp10;

/*
 * Tests single OverrideConstructor advice on an implicit constructor of a record struct.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.Override(builder.Target.Constructors.Single(c => c is { Parameters.Count: 0 }), nameof(Template));
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
public record struct TargetStruct
{
}