#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Implicit_RecordStruct;

/*
 * Tests single OverrideConstructor advice on an implicit constructor of a record struct.
 */

public class OverrideAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.Override( builder.Target.Constructors.Single( c => c is { Parameters.Count: 0 } ), nameof(Template) );
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
public record struct TargetStruct { }